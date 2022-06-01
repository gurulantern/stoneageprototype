using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharController : MonoBehaviour
{
    [SerializeField] PlayerControls _playerControls;
    [SerializeField] private Camera _camera;
    [SerializeField] GameEvent _gatherFruitEvent;
    [SerializeField] GameEvent _observeDone;
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float tiredSpeed = .5f;
    [SerializeField] private LayerMask _layerMask;
    public PlayerStamina _playerStamina;
    public PlayerInventory _playerInventory;
    public FoodCollection _caveCollection;
    private Vector2 moveInput;
    //Bunch of bools for checking if near with Circle colliders and checking player state
    private bool treeNear {get; set;}
    private bool fruitTreeNear {get; set;}
    private bool playerNear {get; set;}
    private bool caveNear {get; set;}
    private bool sleep, observing, gathering, tired;
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
        animator = gameObject.GetComponent<Animator>();
        _playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        _playerStamina.currentStamina = 100;
    }

    void Start()
    {
        sleep = false;
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
            } else if (animator.GetBool("Observe") || animator.GetBool("Gather")) {    
                ChangeStamina(-tireRate);
            } else if (sleep == false && _playerStamina.currentStamina <= tireLimit) {
                //When Awake and stamina is under tire limit, enter tired animation and slow down
                animator.SetBool("Tired", true);
                ChangeStamina(-tireRate);
                rb.MovePosition(rb.position + moveInput * tiredSpeed * Time.fixedDeltaTime);
            } else if (sleep == false) {
                //If sleep is false, decrease stamina and move at base speed
                ChangeStamina(-tireRate);
                rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
            }
        } else {
            _playerControls.Disable();
        }
    }

    //Function for changing stamina over time
    private void ChangeStamina(float rate)
    {
        _playerStamina.currentStamina = Mathf.Clamp(_playerStamina.currentStamina + rate, 0, _playerStamina.maxStamina);
    }

    //Funciton for edible objects to change player stamina
    public void FoodStamina(int amount)
    {
        _playerStamina.currentStamina = Mathf.Clamp(_playerStamina.currentStamina + amount, 0, _playerStamina.maxStamina); 
    }

    //Trigger Exits and Enters to set whether objects are near for interactions
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Fruit Tree") && i == 0) {
            fruitTreeNear = true;
            Debug.Log("fruit tree near");
            i++;
        } else if (other.gameObject.CompareTag("Tree") && i == 0) {
            treeNear = true;
            Debug.Log("tree near");
            i++;
        } else if (other.gameObject.CompareTag("Player") && i == 0) {
            playerNear = true;
            Debug.Log("player near");
            i++;
        } else if (other.gameObject.CompareTag("Cave") && i == 0) {
            caveNear = true;
            Debug.Log("cave near");
            i++;
        } 
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Fruit Tree") && i == 1) {
            fruitTreeNear = false;
            Debug.Log("fruit tree not near");
            i = 0;
        } else if (other.gameObject.CompareTag("Tree") && i == 1) {
            treeNear = false;
            i = 0;
        } else if (other.gameObject.CompareTag("Player") && i == 1) {
            playerNear = false;
            i = 0 ;
        } else if (other.gameObject.CompareTag("Cave") && i == 1) {
            caveNear = false;
            Debug.Log("cave not near");
            i = 0;
        }
    }

    //function for sleeping
    private void OnSleep()
    {
        if(sleep == false) {
            sleep = true;
            animator.SetTrigger("Sleep");
        }
    }
    //function for waking up
    void Wake()
    {
        animator.SetBool("Tired", false);
        animator.SetTrigger("Awake");
        sleep = false;
    }

    //Sets look direction and set speed for animator
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
    }

    //Functions for interacting with food objects (Fruit trees, caves, and other players)
    private void OnFoodAction()
    {
        RaycastHit2D hit;
        Debug.Log("there was a left click at " + Mouse.current.position.ReadValue()); 
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
        if (hit.collider.tag == "Fruit Tree" && fruitTreeNear) {
            //Invokes event for Fruit Tree to animate then send event back
            _gatherFruitEvent?.Invoke();
            Debug.Log("Click event!");
        } else if (hit.collider.tag == "Cave" && caveNear) {
            _caveCollection.AddFood(_playerInventory.food);
            _playerInventory.DropOff();
            Debug.Log("Player food: " + _playerInventory.food);
        } else if (hit.collider.tag == "Player" && playerNear) {
            _playerInventory.AddFood();
        }
    }

    public void Gather() {
        //Triggers at end of Harvesting Animation event
        gameObject.GetComponent<Animator>().SetBool("Gather", true);
        if (gameObject.GetComponent<Animator>().GetBool("Gather")) {
                _playerInventory.AddFood();
        }
        Debug.Log("Player food: " + _playerInventory.food);
    }

    public void StopGather() {
        //Triggers at the end of the gather animation
        gameObject.GetComponent<Animator>().SetBool("Gather", false);
    }    
    
    //Function for right clicking and observing a nearby object
    public void OnObserve()
    {
        RaycastHit2D hit;
        Debug.Log("there was a right click at " + Mouse.current.position.ReadValue());
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
 
        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
        if (hit.collider.tag == "Fruit Tree" && fruitTreeNear) {
            animator.SetBool("Observe", true);
            Debug.Log("Player observes");
        } else if (hit.collider.tag == "Cave" && caveNear) {
            animator.SetBool("Observe", true);
            Debug.Log("Player observes");
        } else if (hit.collider.tag == "Player" && playerNear) {
            animator.SetBool("Observe", true);
            Debug.Log("Player observes");
        }
    }

    // function to notify when Obsering is done
    public void StopObserve()
    {
        animator.SetBool("Observe", false);
        _observeDone?.Invoke();
    }
}
