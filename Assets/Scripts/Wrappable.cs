using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static PlayerController;

public class Wrappable : MonoBehaviour
{

    struct PointAngle
    {
        public float angle;

        public PointAngle(GrapplePoint point)
        {
            this.startPoint = point;
            this.angle = 0;
        }
    }

    public Vector2 centerOffset;

    float totalWrap = 0; 
    private LinkedList<PointAngle> contacts = new LinkedList<PointAngle>();

    float normalizeAngle(float angle)
    {
        angle = angle % 360;
        angle = angle > 0 ? angle : angle + 360;
        return angle;
    }

    public void AddPoint(Vector2 point)
    {
        float angle = 0;
        if (contacts.Count > 0)
        {
            Vector2 center = ((Vector2)(transform.position) + centerOffset);
            Vector2 prevVec = contacts.Last.Value.point - center;
            Vector2 currVec = point - center;

            angle = Vector2.Angle(prevVec, currVec);
            totalWrap += angle;

            if (Mathf.Abs(totalWrap) > 340)
            {
                OnWrap();
            }
        }
        contacts.AddLast(new PointAngle(point,  angle));
    }

    public void OnWrap()
    {
        Debug.Log("Wrapped!");
    }

    public void RemovePoints(int numPoints)
    {
        Vector2 center = ((Vector2)(transform.position) + centerOffset);

        for (int i = 0; i < numPoints; i++)
        {
            if (contacts.Count > 0) {
                totalWrap -= contacts.Last.Value.angle;
                contacts.RemoveLast();
            } else
            {
                ClearPoints();
            }
        }
    }

    public void ClearPoints()
    {
        contacts.Clear();
        totalWrap = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(transform.position + (Vector3)centerOffset, 0.2f);
    }


}
