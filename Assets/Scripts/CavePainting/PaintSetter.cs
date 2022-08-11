using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PaintSetter : MonoBehaviour
{
    protected Vector2 mousePosition;
    Quaternion blankRot = new Quaternion(0, 0, 0 , 0);
    [SerializeField] 
    protected GameObject prefab;
    [SerializeField] 
    protected SpriteRenderer _painterImg;
    [SerializeField]
    private bool player;
    [SerializeField] 
    protected LayerMask _layerMask;
    protected bool flipX = false;
    protected bool flipY = false;
    protected GameObject thisPainting;
    public delegate void Paint(int type, int teamIndex, float posX, float posY, bool flipX, bool flipY);
    public static event Paint paint;
    public Camera _camera;
    public int type;
    private int _wCollisions = 0;
    private Transform collidedWall;
    public BoxCollider2D trigger;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position = mousePosition;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position = mousePosition;


        if (Input.GetMouseButton(0) && _wCollisions > 0)
        {
            thisPainting = Instantiate(prefab, transform.position, blankRot);
            thisPainting.transform.SetParent(collidedWall);
            Destroy(gameObject);
            if (flipX == true) {
                thisPainting.GetComponent<SpriteRenderer>().flipX = true;
            }

            if (flipY == true) {
                thisPainting.GetComponent<SpriteRenderer>().flipY = true;
            }
            if (player == true) {
                thisPainting.GetComponent<SpriteRenderer>().color = GameController.Instance.GetTeamColor(type);
            }
            paint?.Invoke(type, GameController.Instance.GetTeamIndex(ColyseusManager.Instance.CurrentUser.sessionId), 
                thisPainting.transform.localPosition.x, thisPainting.transform.localPosition.y, flipX, flipY);
        }

        if (Input.GetMouseButton(1))
        {
            Destroy(gameObject);
        }

        if (Input.GetKeyDown("x"))
        {
            flipX = !flipX;
            _painterImg.flipX = flipX;
        }

        if (Input.GetKeyDown("y"))
        {
            flipY = !flipY;
            _painterImg.flipY = flipY;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.tag == "Wall" && !other.isTrigger)
        {
            _wCollisions++;
            Debug.Log("Colliding with wall " + _wCollisions);
            collidedWall = other.gameObject.transform;
            Debug.Log("Setting Wall " + other.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Wall" && !other.isTrigger)
        {
            Debug.Log("Not touching wall");
            _wCollisions--;
        }
    }
}
