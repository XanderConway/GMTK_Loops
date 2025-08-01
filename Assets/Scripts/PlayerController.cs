using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngineInternal;

public class PlayerController : MonoBehaviour, PlayerControls.IPlayerActions
{

    public class GrapplePoint
    {
        public Vector2 pos;
        public Vector2 dir;
        public int side;

        public GrapplePoint(Vector2 pos)
        {
            this.pos = pos;
            this.dir = Vector2.zero;
        }

        public GrapplePoint(Vector2 pos, Vector2 dir, int side)
        {
            this.pos = pos;
            this.dir = dir;
            this.side = side;
        }
    }
    private PlayerControls controls;
    Rigidbody2D rb;
    DistanceJoint2D distanceJoint;

    bool grappling;

    public GameObject grapplePoint;
    public LineRenderer rope;
    private LinkedList<GrapplePoint> grapplePoints;
    private float ropeLength;

    public float moveSpeed = 1f;
    public float swingSpeed = 0.1f;
    public float jumpVel = 5f;

    public float airResitance = 0.1f;

    float moveVec;
    bool jumpPressed;

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


    }
    // Start is called before the first frame update
    void Start()
    {

    }

    public void FixedUpdate()
    {
        Vector2 currvel = rb.velocity;

        if (grappling)
        {

            GrapplePoint firstPoint = grapplePoints.Last.Value;
            Vector2 firstDir = firstPoint.pos - (Vector2)transform.position;

            // Check if we can unwrap around any corners
            bool checkPoints = true;
            while(checkPoints && grapplePoints.Count > 1)
            {
                GrapplePoint secondPoint = grapplePoints.Last.Previous.Value;

                Vector2 prevPerp = new Vector2(secondPoint.dir.y, -secondPoint.dir.x);

                float dot = Vector2.Dot(prevPerp, firstDir);
                float currSide = Mathf.Sign(dot);

                if (currSide != 0 && pre)
                {
                    firstPoint = grapplePoints.Last.Value;
                    grapplePoints.RemoveLast();
                }
                else
                {
                    checkPoints = false;
                }
                   
            }

            grapplePoints.Last.Value.dir = firstDir;


            // Check if there are any corners we should wrap around
            RaycastHit2D cornerHit = Physics2D.Raycast(transform.position, firstDir, firstDir.magnitude, LayerMask.GetMask("Terrain"));

            if (cornerHit.collider)
            {
                Vector2 newDir = cornerHit.point - (Vector2)transform.position;

                Vector2 perp = new Vector2(firstPoint.dir.y, -firstPoint.dir.x);

                // This should never be zero
                float sideDot = Vector2.Dot(perp, newDir);

                if (Mathf.Abs(sideDot) > 0)
                {
                    grapplePoints.AddLast(new GrapplePoint(cornerHit.point, firstDir, sideDot > 0));
                }
            }


            // Set the grapple object
            Vector3 rotationPoint = grapplePoints.Last.Value.pos;
            Vector2 grappleDir = rotationPoint - transform.position;

            distanceJoint.enabled = true;
            distanceJoint.connectedAnchor = rotationPoint;
            distanceJoint.distance = grappleDir.magnitude;

            // Move perpendicular to the rope
            Vector2 perpDir = new Vector2(grappleDir.y, -grappleDir.x);
            currvel += perpDir * moveVec * swingSpeed;
        }
        else
        {
            distanceJoint.enabled = false;

            float desiredMotion = moveSpeed * moveVec;

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

            // TODO If this motion causes us to hit a wall, kill velocity
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, groundDist, Vector2.down, groundRad, LayerMask.GetMask("Terrain"));
            bool grounded = hit.collider != null;

            if (jumpPressed)
            {
                if (grounded)
                {
                    currvel.y = jumpVel;
                }
                jumpPressed = false;
            }
        }

        rb.velocity = currvel;
    }

    // Update is called once per frame
    void Update()
    {
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
            grapplePoints.AddLast(new GrapplePoint(hit.point, dir, false));
            grappling = true;
            rope.enabled = true;
            ropeLength = dir.magnitude;
        }
    }

    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Grapple();
        }
        else if (context.canceled)
        {
            grappling = false;
            rope.enabled = false;
            Debug.Log("Ending grapple");
            grapplePoints.Clear();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveVec = context.ReadValue<float>();
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
                Gizmos.DrawLine(p.pos, p.pos + p.dir.Perpendicular1().normalized * 2);
            }
        }

    }


}
