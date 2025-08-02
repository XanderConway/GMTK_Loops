using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static PlayerController;

public abstract class Wrappable : MonoBehaviour
{

    public Vector2 centerOffset;

    [HideInInspector]
    public LinkedList<float> wraps;

    private void OnEnable()
    {
        wraps = new LinkedList<float>();
    }


    public void addAngle(GrapplePoint p1, GrapplePoint p2)
    {
        Vector2 center = ((Vector2)transform.position + centerOffset);
        Vector2 dir1 = p1.pos - center;
        Vector2 dir2 = p2.pos - center;

        float angle = Vector2.Angle(dir1, dir2);

        wraps.Last.Value += angle;
    }

    public abstract void onRelease();


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(transform.position + (Vector3)centerOffset, 0.2f);
    }


}
