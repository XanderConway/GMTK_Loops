using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerControls.IPlayerActions
{

    // TODO: We can store information about sides and directions for more consistent unwrapping
    public class GrapplePoint
    {
        public Vector2 pos;
        public Wrappable hitObject;

        public GrapplePoint(Vector2 pos)
        {
            this.pos = pos;
        }

        public GrapplePoint(Vector2 pos, Wrappable hitObject)
        {
            this.pos = pos;
            this.hitObject = hitObject;
        }

        public GrapplePoint(RaycastHit2D hit)
        {
            this.pos = hit.point;
            this.hitObject = hit.collider.GetComponent<Wrappable>();
        }

        public static bool sameObject(GrapplePoint p1, GrapplePoint p2)
        {
            return p1.hitObject == p2.hitObject && p1.hitObject != null;
        }
    }


    private PlayerControls controls;
    public Rigidbody2D rb;
    DistanceJoint2D distanceJoint;

    bool grappling;


    public LineRenderer rope;
    private LinkedList<GrapplePoint> grapplePoints;
    private HashSet<Wrappable> wrappableObjects; // Objects that are wrappable that have been hit by the current grapple

    private float ropeLength;

    public Camera cam;

    public float moveSpeed = 1f;
    public float swingSpeed = 0.1f;
    public float climbSpeed = 0.2f;
    public float jumpVel = 5f;

    public float airResitance = 0.1f;

    [HideInInspector]
    public Vector2 moveVec;

    bool jumpPressed;

    [HideInInspector] // Made visible for animation purposes
    public bool grounded;

    public float groundRad = 1f;
    public float groundDist = 1f;

    void OnEnable()
    {
        if (controls == null)
        {
            controls = new PlayerControls();
            controls.Player.SetCallbacks(this);
        }
        controls.Player.Enable();
        rb = GetComponent<Rigidbody2D>();
        distanceJoint = GetComponent<DistanceJoint2D>();

        rope.enabled = false;

        grapplePoints = new LinkedList<GrapplePoint>();
        wrappableObjects = new HashSet<Wrappable>();


    }
    // Start is called before the first frame update
    void Start()
    {

    }

    void UnwrapCorners()
    {
        GrapplePoint firstPoint = grapplePoints.Last.Value;
        Vector2 firstDir = firstPoint.pos - (Vector2)transform.position;

        // Check if we can unwrap around any corners
        bool checkPoints = true;
        while (checkPoints && grapplePoints.Count > 1)
        {
            GrapplePoint secondPoint = grapplePoints.Last.Previous.Value;
            Vector2 secondDir = secondPoint.pos - (Vector2)transform.position;

            // Check if we have line of sight to this point
            RaycastHit2D secondHit = Physics2D.Raycast(transform.position, secondDir, secondDir.magnitude * 0.999f, LayerMask.GetMask("Terrain"));

            // 
            if (secondHit.collider == null && Vector2.Dot(firstDir.normalized, secondDir.normalized) > 0.99)
            {

                GrapplePoint lastPoint = grapplePoints.Last.Value;
                grapplePoints.RemoveLast();

                if(lastPoint.hitObject != null)
                {
                    if(grapplePoints.Count == 0 || !GrapplePoint.sameObject(lastPoint, grapplePoints.Last.Value))
                    {
                        lastPoint.hitObject.wraps.RemoveLast();
                    } else
                    {
                        lastPoint.hitObject.addAngle(lastPoint, grapplePoints.Last.Value);
                    }
                } 

                firstPoint = grapplePoints.Last.Value;
                firstDir = firstPoint.pos - (Vector2)transform.position;
            }
            else
            {
                checkPoints = false;
            }
        }
    }

    // Sets up the wrapping for the object
    private void configureWrappings(GrapplePoint newPoint)
    {
        // If this is a wrappable object, add a new wrap if it's not the same as the last, otherwise incremenent current
        if (newPoint.hitObject != null)
        {
            wrappableObjects.Add(newPoint.hitObject);
            if (grapplePoints.Count == 0 || !GrapplePoint.sameObject(newPoint, grapplePoints.Last.Value))
            {
                newPoint.hitObject.wraps.AddLast(0);
            }
            else
            {
                Debug.Assert(newPoint.hitObject.wraps.Count > 0);
                newPoint.hitObject.addAngle(grapplePoints.Last.Value, newPoint);
            }
        }
    }

    void WrapCorners()
    {
        GrapplePoint firstPoint = grapplePoints.Last.Value;
        Vector2 firstDir = firstPoint.pos - (Vector2)transform.position;

        // Check if there are any corners we should wrap around
        RaycastHit2D cornerHit = Physics2D.Raycast(transform.position, firstDir, firstDir.magnitude, LayerMask.GetMask("Terrain"));

        if (cornerHit.collider && (cornerHit.point - firstPoint.pos).magnitude > 0.1f)
        {
            GrapplePoint newPoint = new GrapplePoint(cornerHit);

            configureWrappings(newPoint);

            grapplePoints.AddLast(newPoint);
        }
    }

    public void FixedUpdate()
    {

        Vector2 currvel = rb.velocity;

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, groundDist, Vector2.down, groundRad, LayerMask.GetMask("Terrain"));
        grounded = hit.collider != null;

        if (grappling)
        {
            UnwrapCorners();
            WrapCorners();
        
            // Set the grapple object
            Vector3 rotationPoint = grapplePoints.Last.Value.pos;
            Vector2 grappleDir = (rotationPoint - transform.position);

            distanceJoint.enabled = true;
            distanceJoint.connectedAnchor = rotationPoint;
            distanceJoint.distance = grappleDir.magnitude;

            Vector2 perpDir = new Vector2(grappleDir.y, -grappleDir.x).normalized;

            //if(perpDir.x < 0)
            //{
            //    perpDir = -perpDir;
            //}
            currvel += perpDir * swingSpeed * moveVec.x;


            distanceJoint.distance -= moveVec.y * climbSpeed;


            // Turn movement perpendicular to the rope into momentum, and the rest into climbing
            //currvel += perpDir * Vector2.Dot(perpDir, moveVec) * swingSpeed;
            //distanceJoint.distance -= Vector2.Dot(grappleDir.normalized, moveVec) * climbSpeed;


            // Move along the rope
            //if(climbVec != 0)
            //{
            //    distanceJoint.distance -= climbVec * climbSpeed;
            //}


        }
        else
        {
            distanceJoint.enabled = false;

            float desiredMotion = moveSpeed * moveVec.x;

            if (currvel.x > 0 && desiredMotion > 0)
            {
                currvel.x = Mathf.Max(currvel.x, desiredMotion);
            }
            else if (currvel.x < 0 && desiredMotion < 0)
            {
                currvel.x = Mathf.Min(currvel.x, desiredMotion);
            }
            else
            {
                // TODO, can add some smoothing
                currvel.x = desiredMotion;
            }
        }

        if (jumpPressed)
        {
            if (grounded)
            {
                currvel.y = jumpVel;
            }
            jumpPressed = false;
        }

        rb.velocity = currvel;
    }

    // Update is called once per frame
    void Update()
    {
        if(grapplePressed)
        {
            Grapple();
            grapplePressed = false;
        }

        if(grappleReleased)
        {
            grappling = false;
            rope.enabled = false;

            // Idea: Have an id 0, 1 for the current grapple, and have the wrappable class clear it's own list if it's id is different
            foreach (Wrappable wrapObj in wrappableObjects)
            {
                wrapObj.onRelease();
            }

            wrappableObjects.Clear();
            grapplePoints.Clear();
            grappleReleased = false;
        }

        if (grappling)
        {
            rope.positionCount = grapplePoints.Count + 1;

            LinkedListNode<GrapplePoint> curr = grapplePoints.First;

            
            for(int i = 0; i < grapplePoints.Count; i++)
            {
                rope.SetPosition(i, curr.Value.pos);
                curr = curr.Next;
            }
            rope.SetPosition(grapplePoints.Count, transform.position);
        }
    }

    void Grapple()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (mousePos - (Vector2)transform.position), float.PositiveInfinity, LayerMask.GetMask("Terrain"));

        if (hit.collider)
        {
            Vector2 dir = hit.point - (Vector2)transform.position;

            GrapplePoint newPoint = new GrapplePoint(hit);
            configureWrappings(newPoint);

            grapplePoints.AddLast(newPoint);


            grappling = true;
            rope.enabled = true;
        }
    }

    bool grapplePressed = false;
    bool grappleReleased = false;
    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            grapplePressed = true; // Added to resolve some weird error
        }
        else if (context.canceled)
        {
            grappleReleased = true;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveVec = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpPressed = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere((Vector2)(transform.position + Vector3.down * groundDist), groundRad);

        Gizmos.color = Color.red;

        if (grapplePoints != null)
        {
            foreach (GrapplePoint p in grapplePoints)
            {
                Gizmos.DrawSphere((Vector2)p.pos, 0.2f);
            }
        }

    }
}
