using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleKillSwitch : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }
    
    void Update()
    {
        if (!ps.IsAlive()) {
            Destroy(gameObject);
        }
    }
}
