using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Vector2 camOffset = new Vector2(0f, 0f);
    private Transform target;
    // Start is called before the first frame update

    void Start()
    {
        target = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.position = target.TransformPoint(camOffset);
    }
}
