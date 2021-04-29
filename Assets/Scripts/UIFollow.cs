using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollow : MonoBehaviour {
    public Transform follow;
    public Vector2 positionOffset;
    private RectTransform rectTransform;
    private Camera cam;

    void Start() {
        rectTransform = GetComponent<RectTransform>();
        cam = Camera.main;
    }

    private void LateUpdate() {
        rectTransform.position = RectTransformUtility.WorldToScreenPoint(cam, follow.position) + positionOffset;
    }
}
