using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BlueprintScript : MonoBehaviour
{
    RaycastHit2D hit;
    Vector2 movePoint;
    Vector2 mousePosition;
    public BoxCollider2D trigger;
    Quaternion blankRot = new Quaternion(0, 0, 0 , 0);
    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position = mousePosition;

    }

    // Update is called once per frame
    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position = mousePosition;


        if (Input.GetMouseButton(0))
        {
            Instantiate(prefab, transform.position, blankRot);
            Destroy(gameObject);
        }
    }
}
