using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    private Character character;
    private InputAction moveAction;

    [SerializeField] private List<Follower> followers = new List<Follower>();

    // Order variables
    [SerializeField] private float orderRange;

    void Start()
    {
        character = GetComponent<Character>();
        moveAction = GetComponent<PlayerInput>().actions.FindAction("Move");
        
        SetupFollowerManagement();
    }
    
    void Update()
    {
        Move();
    }

    private void Move() {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        character.Move(new Vector3(direction.x, 0, direction.y).normalized);
    }

    private void OnAction(InputValue value) {
        character.SwingTool();
    }

    private void OnSecondaryAction(InputValue value) {
        character.PickUpOrDropOff();
    }

    private void SetupFollowerManagement() {
        if (character.Tool != null) {
            character.Tool.onHit += OrderHarvest;
        }
        character.onPickUp += OrderGather;
        character.onDropOff += OrderDropOff;
    }

    private void OrderHarvest() {
        LayerMask layerMask = LayerMask.GetMask("ResourcePoints");
        Collider[] colliders = Physics.OverlapSphere(transform.position, orderRange, layerMask);

        List<Collider> collidersOfInterest = new List<Collider>(colliders);
        if (collidersOfInterest.Count > character.Tool.RecentlyHit.Count) {
            foreach (ResourcePoint rp in character.Tool.RecentlyHit) {
                collidersOfInterest.Remove(rp.GetComponent<Collider>());
            }
        }

        if (collidersOfInterest.Count > 0) {
            // Order harvest, but sort by distance to resource point for each follower.
            foreach (Follower follower in followers) {
                if (collidersOfInterest.Count == 0) {
                    break;
                }
                collidersOfInterest.Sort((x, y) => Vector3.Distance(x.transform.position, follower.transform.position) > Vector3.Distance(y.transform.position, transform.position) ? 1 : -1);
                follower.TakeOrder(Follower.Behaviour.Harvest, collidersOfInterest[0].gameObject);
                collidersOfInterest.RemoveAt(0);
            }
        }
    }

    private void OrderGather() {
        LayerMask layerMask = LayerMask.GetMask("PickUps");
        Collider[] colliders = Physics.OverlapSphere(transform.position, orderRange, layerMask);

        // Remove the PickUp carried by the Player's character
        List<Collider> collidersOfInterest = new List<Collider>(colliders);
        collidersOfInterest.Remove(character.CarriedPickUp.GetComponent<Collider>());

        if (collidersOfInterest.Count > 0) {
            // Order gather, but sort by distance to pickup for each follower.
            foreach (Follower follower in followers) {
                if (collidersOfInterest.Count == 0) {
                    break;
                }
                collidersOfInterest.Sort((x, y) => Vector3.Distance(x.transform.position, follower.transform.position) > Vector3.Distance(y.transform.position, transform.position) ? 1 : -1);
                follower.TakeOrder(Follower.Behaviour.Gather, collidersOfInterest[0].gameObject);
                collidersOfInterest.RemoveAt(0);
            }
        }
    }

    private void OrderDropOff() {
        LayerMask layerMask = LayerMask.GetMask("DropOffs");
        List<Collider> colliders = new List<Collider>(Physics.OverlapSphere(transform.position, orderRange, layerMask));

        if (colliders.Count > 0) {
            // Order gather, but sort by distance to dropoff for each follower.
            foreach (Follower follower in followers) {
                if (follower.Character.Carrying) {
                    colliders.Sort((x, y) => Vector3.Distance(x.transform.position, follower.transform.position) > Vector3.Distance(y.transform.position, transform.position) ? 1 : -1);
                    follower.TakeOrder(Follower.Behaviour.Deliver, colliders[0].gameObject);
                }
            }
        }
    }
}
