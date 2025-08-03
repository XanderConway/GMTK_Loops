using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageIK : MonoBehaviour
{
    public PlayerController controller;
    public List<Fabrik2D> legs;
    public List<Transform> targets;

    public float castLength = 1f;

    public float cycleLength = 0.2f;
    private int currLeg = 0;
    private float timer = 0f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {

    }

    // Update is called once per frame
    bool prevGrounded = false;

    void updateLeg(int i)
    {
        RaycastHit2D hit = Physics2D.Raycast(targets[i].position, Vector2.down, castLength, LayerMask.GetMask("Terrain"));

        if (hit.collider != null)
        {
            legs[i].target = hit.point;
        }
        else
        {
            legs[i].target = targets[i].transform.position + Vector3.down * castLength * 0.8f;
        }
    }
    void Update()
    {

        if (controller.grounded && prevGrounded)
        {
            timer += Time.deltaTime;

            if (timer > cycleLength)
            {
                updateLeg(currLeg);
                timer = 0;
                currLeg = currLeg == legs.Count - 1 ? 0 : currLeg + 1;
                //legs[currLeg].Fabrik();
            }
        } else if(controller.grounded && !prevGrounded)
        {
            // Ground all the legs
            for(int i = 0; i < legs.Count; i++)
            {
                updateLeg(i);
            }
        }
        else
        {
            // Just set a pose for swinging
            for (int i = 0; i < legs.Count; i++)
            {
                legs[i].target = targets[i].transform.position + (Vector3)controller.rb.velocity * -0.05f;
                //(Vector2)targets[i].position + Vector2.down * castLength * 0.8f;
            }

        }

        prevGrounded = controller.grounded;
    }

    void OnDrawGizmos()
    {

        for (int i = 0; i < legs.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targets[i].transform.position, 0.1f);
            Gizmos.DrawLine(targets[i].transform.position, targets[i].transform.position + Vector3.down * castLength);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(legs[i].target, 0.1f);
        }
    }


}
