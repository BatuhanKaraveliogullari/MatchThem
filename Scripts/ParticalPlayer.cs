using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticalPlayer : MonoBehaviour
{
    public ParticleSystem[] allParticles;

    public float lifeTime = 1f;

    public bool destroyImmediately = false;

    void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();

        if(destroyImmediately)
        {
            Destroy(gameObject, lifeTime);

            destroyImmediately = false;
        }
    }

    public void Play()
    {
        foreach (ParticleSystem ps in allParticles)
        {
            ps.Stop();

            ps.Play();

            destroyImmediately = true;
        }

        Destroy(gameObject, lifeTime);
    }
}
