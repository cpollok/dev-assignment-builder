using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private Transform follow;
    [SerializeField] private Vector3 positionOffset;

    void LateUpdate() {
        transform.position = follow.position + positionOffset;
    }
}
