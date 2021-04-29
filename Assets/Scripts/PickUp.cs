using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    private AudioSource audioSource;

    public bool onGround = true;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    public void GetPickedUp() {
        audioSource.Play();
        onGround = false;
        gameObject.layer = LayerMask.NameToLayer("Tools");
    }
}
