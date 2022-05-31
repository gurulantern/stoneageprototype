using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class CharController : MonoBehaviour
{
    [SerializeField] private PlayerControls _playerControls;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameEvent _observeDone;
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float tiredSpeed = .5f;
    [SerializeField] private LayerMask _layerMask;
    public PlayerStamina _playerStamina;
    private Vector2 moveInput;
    //Bunch of bools for checking if near with Circle colliders and checking player state
    private bool treeNear, fruitTreeNear, playerNear, caveNear, sleep, observing, gathering, tired;
    //Limit for when player enters tire state
    public float tireLimit = 10f;
    public float tireRate = .01f;
    public float restoreRate = .5f;
    //Iterator variable for debugging Trigger Enter and Exit
    private int i = 0;
    Rigidbody2D rb;
    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);
    
    private void Awake() {
        _playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        _playerStamina.currentStamina = 100;
    }

    void Start()
    {
        sleep = false;
    }

    void Update()
    {

    }
    void FixedUpdate()
    {
        if (GameController.instance.gamePlaying)
        {
            if (_playerStamina.currentStamina == _playerStamina.maxStamina && sleep == true) {
                //When stamina is full after sleeping call Wake
                Wake();
            } else if (sleep == true) {
                //When sleep is true and stamina is not full, restore stamina
                ChangeStamina(restoreRate);
                Debug.Log("Sleeping and restoring stamina");
            } else if (sleep == false && _playerStamina.currentStamina <= tireLimit) {
                //When Awake and stamina is under tire limit, enter tired animation and slow down
                animator.SetBool("Tired", true);
                ChangeStamina(-tireRate);
                speed = .5f;
                rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
            } else if (sleep == false) {
                //If sleep is false, decrease stamina and move at base speed
                ChangeStamina(-tireRate);
                speed = 2.5f;
                rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
            }
        } else {
            _playerControls.Disable();
        }
    }

    private void ChangeStamina(float rate)
    {
        _playerStamina.currentStamina = Mathf.Clamp(_playerStamina.currentStamina + rate, 0, _playerStamina.maxStamina);
    }

    public void FoodStamina(int amount)
    {
        _playerStamina.currentStamina = Mathf.Clamp(_playerStamina.currentStamina + amount, 0, _playerStamina.maxStamina); 
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Fruit Tree") && i == 0) {
            fruitTreeNear = true;
            i++;
        } else if (other.gameObject.CompareTag("Tree") && i == 0) {
            treeNear = true;
            i++;
        } else if (other.gameObject.CompareTag("Player") && i == 0) {
            playerNear = true;
            i++;
        } else if (other.gameObject.CompareTag("Cave") && i == 0) {
            caveNear = true;
            i++;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Fruit Tree") && i == 1) {
            fruitTreeNear = false;
            i++;
        } else if (other.gameObject.CompareTag("Tree") && i == 1) {
            treeNear = false;
            i++;
        } else if (other.gameObject.CompareTag("Player") && i == 1) {
            playerNear = false;
            i++;
        } else if (other.gameObject.CompareTag("Cave") && i == 1) {
            caveNear = false;
            i++;
        }
    }
    private void OnSleep()
    {
        if(sleep == false) {
            sleep = true;
            animator.SetTrigger("Sleep");
            Debug.Log("Player is asleep");
        }
    }

    void Wake()
    {
        animator.SetBool("Tired", false);
        animator.SetTrigger("Awake");
        sleep = false;
        Debug.Log("Player is awake");
    }

    public void OnObserve()
    {
        Debug.Log("there was a click at " + Mouse.current.position.ReadValue());
 
        RaycastHit2D hit;
 
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
 
        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
        Debug.Log(hit.collider);
        animator.SetBool("Observe", true);
        Debug.Log("Player observes");
    }

    public void StopObserve()
    {
        animator.SetBool("Observe", false);
        _observeDone?.Invoke();
    }

    private void OnMove(InputValue value) 
    {
        moveInput = value.Get<Vector2>();

        if(!Mathf.Approximately(moveInput.x, 0.0f) || !Mathf.Approximately(moveInput.y, 0.0f))
        {
            lookDirection.Set(moveInput.x, moveInput.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", moveInput.magnitude);

        Debug.Log("Moving");
    }

    private void OnFoodAction()
    {
        Debug.Log("there was a click at " + Mouse.current.position.ReadValue());
 
        RaycastHit2D hit;
 
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
 
        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
        Debug.Log(hit.collider);
    }
}
