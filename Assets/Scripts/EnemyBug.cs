using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBug : Wrappable
{
    // Start is called before the first frame update
    public GameObject splat;

    public override void onRelease()
    {
        LinkedListNode<float> curr = wraps.First;

        bool wrapped = false;
        while(curr != null)
        {
            if(Mathf.Abs(curr.Value) > 300)
            {
                wrapped = true;
                break;
            }
            curr = curr.Next;
        }

        wraps.Clear();

        if(wrapped)
        {
            Instantiate(splat, transform.position + (Vector3)centerOffset, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
