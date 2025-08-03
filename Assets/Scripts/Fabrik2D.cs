using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Fabrik2D : MonoBehaviour
{
    // Start is called before the first frame update
    struct BoneData
    {
        public float length;
        public Transform bone;

        public BoneData(float length, Transform bone)
        {
            this.length = length;
            this.bone = bone;
        }
    }

    public Vector3 target = Vector3.zero;
    public int iters = 4;
    public List<Vector2> rotConstraints;

    List<BoneData> bones;



    void Start()
    {
        // Build Bones

        // To get armature->bone1
        Transform curr = transform.GetChild(0).GetChild(0);
        bones = new List<BoneData>();
        while (curr.childCount > 0)
        {
            Transform child = curr.GetChild(0);
            float length = (curr.position - child.position).magnitude;
            bones.Add(new BoneData(length, curr));
            curr = child;
        }
        bones.Add(new BoneData(0, curr));

    }

    public void Fabrik()
    {
        // Set positions
        int n = bones.Count;
        Vector2[] positions = new Vector2[n];
        for (int i = 0; i < n; i++) {
            positions[i] = bones[i].bone.position;
        }


        for (int j = 0; j < iters; j++)
        {
            // backward
            positions[n - 1] = target;
            for (int i = n - 2; i >= 0; i--)
            {
                Vector2 dir = (positions[i] - positions[i + 1]).normalized;
                positions[i] = positions[i + 1] + dir * bones[i].length;
            }

            // forward
            positions[0] = bones[0].bone.position;
            for (int i = 1; i < n; i++)
            {
                Vector2 dir = (positions[i] - positions[i - 1]).normalized;
                positions[i] = positions[i - 1] + dir * bones[i - 1].length;

            }

            // Early return
            if ((positions[n - 1] - (Vector2)target).magnitude < 0.05)
            {
                break;
            }
        }

        // set rotations
        for (int i = 0; i < n - 1; ++i)
        {
            Vector2 dir = positions[i + 1] - positions[i];

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            bones[i].bone.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // set positions
        for (int i = 1; i < n; ++i)
        {
            bones[i].bone.position = positions[i];
        }

    }

    // Update is called once per frame
    void Update()
    {
        Fabrik();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if(bones != null)
        {
            for (int i = 0; i < bones.Count; ++i)
            {
                Gizmos.DrawSphere(bones[i].bone.position, 0.02f);
            }
        }
    }
}
