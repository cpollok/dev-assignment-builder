using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropOff : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    [SerializeField] private GameObject resourceCounter;
    [SerializeField] private Text amountText;

    private int resourceAmount;

    private void Start() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void UpdateCounter() {
        if (resourceAmount > 0) {
            if (!resourceCounter.activeSelf) {
                resourceCounter.SetActive(true);
            }
            amountText.text = resourceAmount.ToString();
        }
        else {
            resourceCounter.SetActive(false);
        }
    }

    public bool ReceivePickUp(PickUp pickUp) {
        // Returns a boolean, so it could theoretically reject a pickup delivered.
        // Makes it easier to extend for multiple resource/pickup types.
        resourceAmount++;
        UpdateCounter();
        // PlayAnim
        animator.SetTrigger("Pulse");
        audioSource.Play();
        return true;
    }
}
