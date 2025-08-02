using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryAfterLifetime : MonoBehaviour
{
    // Start is called before the first frame update
    ParticleSystem particles;

    private void OnEnable()
    {
        particles = GetComponent<ParticleSystem>();
        Destroy(gameObject, particles.main.duration);

    }
}
