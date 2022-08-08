using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PaintSetter : MonoBehaviour
{
    RaycastHit2D hit;
    Ray ray;
    Vector2 movePoint;
    Vector2 mousePosition;
    public int prefabInt;
    Quaternion blankRot = new Quaternion(0, 0, 0 , 0);

    private GameObject thisPainting;
    [SerializeField] Camera _camera;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private GameObject prefab;

    public delegate void Paint(Painting painting);
    public static event Paint paint;

    // Start is called before the first frame update
    void Start()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position = mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);

        mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position = mousePosition;

        if (Input.GetMouseButton(0))
        {
            thisPainting = Instantiate(prefab, transform.position, blankRot);
            //thisPainting.GetComponent<Painting>().SetOwnerTeam(GameController.Instance.GetTeamIndex(ColyseusManager.Instance.CurrentUser.sessionId));
            paint?.Invoke(thisPainting.GetComponent<Painting>());
        }

        if (Input.GetMouseButton(1))
        {
            Destroy(gameObject);
        }
    }

    public void SetPainting(GameObject painting)
    {
        prefab = painting;
    }

    public void Flip()
    {

    }
}
