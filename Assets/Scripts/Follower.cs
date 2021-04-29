using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(NavMeshAgent))]
public class Follower : MonoBehaviour
{
    public enum Behaviour {
        Follow,
        PullCart,
        Harvest,
        Gather,
        Deliver
    }

    private Character character;
    public Character Character { get { return character; } }
    private NavMeshAgent navAgent;

    [SerializeField] private Player player;
    [SerializeField] private Behaviour behaviour = Behaviour.Follow;
    [SerializeField] private GameObject target;

    // Follow variables
    [SerializeField] private float followDistance;

    // Pull variables
    [SerializeField] private float followDistanceWithCart;
    [SerializeField] private float cartPullSpeed;

    // Harvest variables
    [SerializeField] private float harvestDistance;

    // Gather variables
    [SerializeField] private float gatherDistance;

    // Deliver variables
    [SerializeField] private float deliverDistance;


    void Start()
    {
        character = GetComponent<Character>();
        navAgent = GetComponent<NavMeshAgent>();
    }
    
    void Update()
    {
        Behave();
    }

    public void TakeOrder(Behaviour newBehaviour, GameObject newTarget) {
        if (behaviour == Behaviour.Follow) {
            if (character.Carrying && newBehaviour != Behaviour.Deliver) {
                return;
            }
            behaviour = newBehaviour;
            target = newTarget;
        }
    }

    private void Behave() {
        switch (behaviour) {
            case Behaviour.Follow:
                Follow();
                break;
            case Behaviour.PullCart:
                PullCart();
                break;
            case Behaviour.Harvest:
                Harvest();
                break;
            case Behaviour.Gather:
                Gather();
                break;
            case Behaviour.Deliver:
                Deliver();
                break;
            default:
                break;
        }
    }

    private void Follow() {
        if (TargetDistance() > followDistance) {
            MoveCloser();
        }
        else {
            character.Move(Vector3.zero);
            character.Turn(target.transform.position-transform.position);
        }
    }

    private void PullCart() {
        if (TargetDistance() > followDistanceWithCart) {
            NavMeshPath path = new NavMeshPath();
            navAgent.CalculatePath(target.transform.position, path);
            if (path.corners.Length >= 2) {
                character.MoveWithSpeed((path.corners[1] - transform.position).normalized, cartPullSpeed);
            }
        }
        else {
            character.Move(Vector3.zero);
        }
    }

    private void Harvest() {
        ResourcePoint rp = target.GetComponent<ResourcePoint>();
        if (rp.Alive) {
            if (TargetDistance() > harvestDistance) {
                MoveCloser();
            }
            else {
                character.Move(Vector3.zero);
                TurnToTarget();
                character.SwingTool();
            }
        }
        else {
            ReturnToDefaultBehaviour();
        }
    }

    private void Gather() {
        PickUp pu = target.GetComponent<PickUp>();
        if (pu.onGround) {
            if (TargetDistance() > gatherDistance) {
                MoveCloser();
            }
            else {
                character.Move(Vector3.zero);
                TurnToTarget();
                character.PickUpOrDropOff();
            }
        }
        else {
            ReturnToDefaultBehaviour();
        }
    }

    private void Deliver() {
        DropOff dropOff = target.GetComponent<DropOff>();
        if (character.Carrying) {
            if (TargetDistance() > deliverDistance) {
                MoveCloser();
            }
            else {
                character.Move(Vector3.zero);
                TurnToTarget();
                character.PickUpOrDropOff();
            }
        }
        else {
            ReturnToDefaultBehaviour();
        }
    }

    private void ReturnToDefaultBehaviour() {
        behaviour = Behaviour.Follow;
        target = player.gameObject;
    }

    private float TargetDistance() {
        return Vector3.Distance(target.transform.position, transform.position);
    }

    private void MoveCloser() {
        NavMeshPath path = new NavMeshPath();
        navAgent.CalculatePath(target.transform.position, path);
        if (path.corners.Length >= 2) {
            character.Move((path.corners[1] - transform.position).normalized);
        }
    }

    private void TurnToTarget() {
        character.Turn(target.transform.position - transform.position);
    }
}
