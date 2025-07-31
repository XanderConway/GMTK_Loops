using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngineInternal;

public class PlayerController : MonoBehaviour, PlayerControls.IPlayerActions
{
    private PlayerControls controls;
    Rigidbody2D rb;
    DistanceJoint2D distanceJoint;

    bool grappling;

    public GameObject grapplePoint;

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
            distanceJoint.enabled = true;
            distanceJoint.connectedAnchor = grapplePoint.transform.position;
            distanceJoint.distance = (transform.position - grapplePoint.transform.position).magnitude;

            Vector2 grappleDir = grapplePoint.transform.position - transform.position;

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
            } else if (currvel.x < 0 && desiredMotion < 0)
            {
                currvel.x = Mathf.Min(currvel.x, desiredMotion);
            } else 
            {
                // TODO, can add some smoothing
                currvel.x = desiredMotion;
            }

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, groundDist, Vector2.down, groundRad, LayerMask.GetMask("Terrain"));
            bool grounded = hit.collider != null;

            if(jumpPressed)
            {
                if(grounded)
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

    }

    void Grapple()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (mousePos - (Vector2)transform.position), float.PositiveInfinity, LayerMask.GetMask("Terrain"));

        if(hit.collider)
        {
            grapplePoint.transform.position = hit.point;
            grappling = true;
        }
    }

    public void OnGrapple(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            Grapple();
        } else if(context.canceled)
        {
            grappling = false;
            Debug.Log("Ending grapple");
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveVec = context.ReadValue<float>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            jumpPressed = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere((Vector2)(transform.position + Vector3.down * groundDist), groundRad);
    }


}
