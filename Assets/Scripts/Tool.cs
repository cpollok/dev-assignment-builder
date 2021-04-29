using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Tool : MonoBehaviour
{
    [SerializeField] private ParticleSystem trail;
    private Collider coll;

    [SerializeField] private int damage;

    private List<ResourcePoint> recentlyHit;  // List of all the recently hit resource points to avoid double hits
    public List<ResourcePoint> RecentlyHit { get { return recentlyHit; } }

    public delegate void OnHit();
    public OnHit onHit;
    
    void Start()
    {
        coll = GetComponent<Collider>();
        recentlyHit = new List<ResourcePoint>();
    }

    public void StartSwing() {
        trail.Play();
        coll.enabled = true;
    }

    public void OnEndOfForwardSwing() {
        coll.enabled = false;
    }

    public void EndSwing() {
        recentlyHit.Clear();
    }

    public void Hit(ResourcePoint rp) {
        recentlyHit.Add(rp);
        rp.GetHit(damage);
        onHit?.Invoke();
    }

    public bool AlreadyHit(ResourcePoint rp) {
        return recentlyHit.Contains(rp);
    }
}
