using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePoint : MonoBehaviour
{
    [SerializeField] private GameObject resourcePrefab; // Which GO should be spawned, when this point is harvested?
    [SerializeField] private Transform spawnPoint; // Where to spawn the GO
    [SerializeField] private GameObject fxPrefab; // ParticleSystem-Prefab for spawn animation
    [SerializeField] private GameObject destroyGO; // GO to destroy when resource is spawned

    private Animator animator;
    private Collider coll;
    private AudioSource audioSource;

    [SerializeField] private int health;
    private bool alive = true;
    public bool Alive { get { return alive; } }

    private void Start() {
        animator = GetComponent<Animator>();
        coll = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update() {
        if (alive && health <= 0) {
            Harvest();
        }
    }

    public void GetHit(int damage) {
        if (alive) {
            audioSource.Play();
            animator.SetTrigger("Hit");
            health -= damage;
        }
    }

    public void Harvest() {
        alive = false;
        animator.SetTrigger("Harvested");
    }

    public void SpawnResource() {
        if (fxPrefab) {
            Instantiate(fxPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        Instantiate(resourcePrefab, spawnPoint.position, spawnPoint.rotation);
        Destroy(destroyGO);
        coll.enabled = false;
    }

    private void OnTriggerEnter(Collider other) {
        Tool tool = other.GetComponent<Tool>();
        if (tool && !tool.AlreadyHit(this)) {
            tool.Hit(this);
        }
    }
}
