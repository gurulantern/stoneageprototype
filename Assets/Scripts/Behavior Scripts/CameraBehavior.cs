using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField]
    private Transform target = null;

    [SerializeField]
    private Transform cameraTransform = null;
    public Vector2 camOffset = new Vector2(0f, 0f);
    void LateUpdate()
    {
        this.transform.position = target.TransformPoint(camOffset);
    }

    public void SetFollow(Transform trans)
    {
        target = trans;
    }
}
