using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Character : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;

    // Movement attributes
    [SerializeField] private float characterSpeed;
    private Vector3 playerVelocity;
    private float gravityValue = -9.81f;

    // Action attributes
    [SerializeField] Transform toolMount;
    [SerializeField] Transform backMount;
    
    [SerializeField] private Tool tool;
    private bool swinging;

    [SerializeField] private Transform carryMount;
    private PickUp carriedPickUp;
    [SerializeField] private bool carrying;

    // Properties
    public Tool Tool { get { return tool; } }
    public PickUp CarriedPickUp { get { return carriedPickUp; } }
    public bool Carrying { get { return carrying; } }

    // Delegates
    public delegate void OnPickUp();
    public OnPickUp onPickUp;

    public delegate void OnDropOff();
    public OnDropOff onDropOff;

    // Debugging
    [SerializeField] private bool showGizmos = false;

    private bool ReadyForAction {
        get { return !swinging && !carrying; }
    }
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        ApplyVelocity();
    }

    private void ApplyVelocity() {
        // Apply velocity to let character fall to ground if it is in the air for some reason.
        if (controller.isGrounded && playerVelocity.y < 0) {
            playerVelocity.y = 0f;
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Move(Vector3 direction) {
        Vector3 flatDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
        MoveWithSpeed(flatDirection, characterSpeed);
    }

    public void MoveWithSpeed(Vector3 direction, float speed) {
        Vector3 flatDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
        if (flatDirection.magnitude > 0) {
            animator.SetBool("Walking", true);
            Turn(flatDirection);
        }
        else {
            animator.SetBool("Walking", false);
        }

        controller.Move(flatDirection * speed * Time.deltaTime);
    }

    public void Turn(Vector3 direction) {
        transform.forward = Vector3.ProjectOnPlane(direction, Vector3.up);
    }

    public void SwingTool() {
        if (ReadyForAction) {
            audioSource.Play();
            swinging = true;
            animator.SetTrigger("Swing");
            tool.StartSwing();
        }
    }

    public void OnEndOfForwardSwing() {
        tool.OnEndOfForwardSwing();
    }

    public void OnEndSwing() {
        tool.EndSwing();
        swinging = false;
    }

    public void PickUpOrDropOff() {
        if(ReadyForAction) {
            TryPickUp();
        }
        else if (carrying) {
            TryDropOff();
        }
    }

    private void PickUp(PickUp pickUp) {
        // Put tool on back
        tool.transform.SetParent(backMount, false);
        tool.transform.localPosition = Vector3.zero;

        // Handle pickup
        carrying = true;
        carriedPickUp = pickUp;
        //pickUp.onGround = false;
        pickUp.GetPickedUp();
        animator.SetBool("Carrying", true);
        onPickUp?.Invoke();

        // Put PickUp into hands of character
        pickUp.transform.SetParent(carryMount, false);
        pickUp.transform.localPosition = Vector3.zero;
        pickUp.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    private void DropOffCarriedPickUp(DropOff dropOff) {
        if (dropOff.ReceivePickUp(carriedPickUp)) {
            //Handle dropoff
            Destroy(carriedPickUp.gameObject);
            carriedPickUp = null;
            carrying = false;
            animator.SetBool("Carrying", false);
            onDropOff?.Invoke();

            // Put tool back in hand
            tool.transform.SetParent(toolMount, false);
            tool.transform.localPosition = Vector3.zero;
        }
    }

    // This and TryDropOff could be one method, but I did not want to mess with function references because
    // it can get less readable.
    private void TryPickUp() {
        // Check for objects to pick up in front of the character.
        // If there are multiple, take the closest one.
        // Otherwise nothing happens.
        LayerMask layerMask = LayerMask.GetMask("PickUps");
        Collider[] colliders = CheckForObjectsInFront(layerMask);
        if (colliders.Length > 0) {
            float minDistance = float.MaxValue;
            PickUp nearest = null;
            foreach (Collider coll in colliders) {
                PickUp collPickUp = coll.GetComponent<PickUp>();
                if (collPickUp.onGround) {
                    float distance = Vector3.Distance(transform.position, coll.transform.position);
                    if (distance < minDistance) {
                        minDistance = distance;
                        nearest = collPickUp;
                    }
                }
            }
            if (nearest != null) {
                PickUp(nearest);
            }
        }
    }

    private void TryDropOff() {
        // Check for DropOff site in front of the character.
        // If there are (for whatever reason) multiple, take the closest one.
        LayerMask layerMask = LayerMask.GetMask("DropOffs");
        Collider[] colliders = CheckForObjectsInFront(layerMask);
        if (colliders.Length > 0) {
            float minDistance = float.MaxValue;
            Collider nearest = null;
            foreach (Collider coll in colliders) {
                float distance = Vector3.Distance(transform.position, coll.transform.position);
                if (distance < minDistance) {
                    minDistance = distance;
                    nearest = coll;
                }
            }
            DropOffCarriedPickUp(nearest.GetComponentInParent<DropOff>());
        }
    }

    private Collider[] CheckForObjectsInFront(LayerMask layerMask) {
        Vector3 boxCenter = transform.position + transform.forward + transform.up;
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, Vector3.one, transform.rotation, layerMask);
        return hitColliders;
    }

    void OnDrawGizmos() {
        //Draw the OverlapBox for checking of objects as a gizmo to show where it currently is testing.
        if (showGizmos) {
            Gizmos.color = Color.red;
            // Doesn't seem like there's a rotatable box gizmos sadly. So this is not exact for when the character is rotated.
            Gizmos.DrawWireCube(transform.position + transform.forward + transform.up, Vector3.one * 2);
        }
    }
}
