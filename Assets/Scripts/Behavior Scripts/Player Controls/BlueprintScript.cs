using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class BlueprintScript : MonoBehaviour
{
    RaycastHit2D hit;
    Vector2 movePoint;
    Vector2 mousePosition;
    public BoxCollider2D trigger;
    public int prefabInt;
    Quaternion blankRot = new Quaternion(0, 0, 0 , 0);
    [SerializeField] private GameObject prefab;
    [SerializeField] private SpriteRenderer _blueprintImg;
    [SerializeField] private Color green;
    [SerializeField] private Color red;

    private int _oCollisions = 0;
    private int _cCollisions = 0;
    [SerializeField] private int intType;
    public float cost;
    public delegate void CreateObject(int type, float cost, Scorable scorable);
    public static event CreateObject createObject;
    [SerializeField] private GameObject currentScorable; 
    [SerializeField] private Transform _environmentController;

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

        if (_oCollisions == 0 && _cCollisions == 0) 
        {
            _blueprintImg.color = green;
        } else {
            _blueprintImg.color = red;
        }

        if (Input.GetMouseButton(0) && _oCollisions == 0 && _cCollisions == 0)
        {
            currentScorable = Instantiate(prefab, transform.position, blankRot);
            currentScorable.GetComponent<Scorable>().SetOwnerTeam(GameController.Instance.GetTeamIndex(ColyseusManager.Instance.CurrentUser.sessionId));
            Destroy(gameObject);
            createObject?.Invoke(intType, cost, currentScorable.GetComponent<Scorable>());
        }

        if (Input.GetMouseButton(1))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.tag == "Creeks")
        {
            _cCollisions++;
        }

        if (!other.isTrigger)
        {
            _oCollisions++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Creeks")
        {
            _cCollisions--;
        }

        if (!other.isTrigger)
        {
            _oCollisions--;
        }
    }
}
