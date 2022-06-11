using UnityEngine;
using UnityEngine.InputSystem;
using Colyseus;
using System;
using System.Collections;
using System.Collections.Generic;
using LucidSightTools;

public class CharControllerMulti : MonoBehaviour
{
    public delegate void OnPlayerActivated(CharControllerMulti playerController);
    public static event OnPlayerActivated onPlayerActivated;
    public delegate void OnPlayerDeactivated(CharControllerMulti playerController);
    public static event OnPlayerDeactivated onPlayerDeactivated;
    [SerializeField] PlayerControls _playerControls;
    [SerializeField] private Camera _camera;
    [SerializeField] GameEvent _gatherFruitEvent;
    [SerializeField] GameEvent _dropOffEvent;
    [SerializeField] GameEvent _observeDone;
    [SerializeField] private LayerMask _layerMask;
    public NetworkedEntity owner;
    public Transform cameraTransform;
    public PlayerStats _playerStats;
    public float maxStamina, currentStamina, speed, tiredSpeed, tireLimit, tireRate, restoreRate;
    public int food, wood;
    public Vector2 MoveInput
    {
        get { return moveInput;}
    }
    private Vector2 moveInput;
    //Bunch of bools for checking if near with Circle colliders and checking player state
    private bool treeNear, fruitTreeNear, playerNear, caveNear;
    private bool sleep, observing, gathering, tired;
    //Iterator variable for debugging Trigger Enter and Exit
    private int i = 0;
    private int teamIndex = -1;
    public int TeamIndex
    {
        get 
        { 
            return teamIndex; 
        }
    }
    public Color defaultColor = Color.white; 
    public Color[] teamColors;
    public Renderer[] playerRenderers;
    Rigidbody2D rb;
    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);
    private Vector2 spawnPosition;
    public event Action ChangedFood;
    public event Action ChangedWood;
    
    private void Awake() {
        animator = gameObject.GetComponent<Animator>();
        _playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        maxStamina = _playerStats.MaxStamina;
        currentStamina = _playerStats.MaxStamina;
        speed = _playerStats.Speed;
        tiredSpeed = _playerStats.TiredSpeed;
        tireLimit = _playerStats.TireLimit;
        tireRate = _playerStats.TireRate;
        restoreRate = _playerStats.RestoreRate;
        food = _playerStats.Food;
        wood = _playerStats.Wood;
    }

    private void OnEnable() 
    {
        ///GameController.onUpdateClientTeam += OnTeamUpdated;    
    }

    private void OnDisable() 
    {
        ///GameController.onUpdateClientTeam -= OnTeamUpdated;
        ///onPlayerDeactivated?.Invoke(this);    
    }

    private void Start()
    {
        sleep = false;

        ///StartCoroutine("WaitForConnect");
    }
/*
    private void OnTeamUpdated(int team, string id)
    {
        //If we're in team death match, we need our team index
        if (id.Equals(OwnerId))
        {
            SetTeam(team);
        }
    }

    public override void InitiView(NetworkedEntity entity)
    {
        base.InitiView(entity);
        onPlayerActivated?.Invoke(this);

        //If we're in team death match, we need our team index
        if (!GameController.Instance)
        {
            SetTeam(GameController.Instance.GetTeamIndex(OwnerId));
        }

        if (IsMine)
        {
            spawnPosition = transform.position;
        }
    }

    private void SetTeam(int idx)
    {
        teamIndex = idx;
        if (teamIndex >= 0)
        {
            SetPlayerColor(teamColors[teamIndex]);
        }
    }

    public void SetPlayerColor(Color color)
    {
        for (int i = 0; i < playerRenderers.Length; ++i)
        {
            playerRenderers[i].material.color = color;
        }
    }
    public override void OnEntityRemoved()
    {
        base.OnEntityRemoved();
        LSLog.LogImportant("REMOVING ENTITY", LSLog.LogColor.lime);
        Destroy(this.gameObject);
    }
    IEnumerator WaitForConnect() 
    {
        if (ColyseusManager.Instance.CurrentUser != null && !IsMine) yield break;

        while (!ColyseusManager.Instance.isInRoom)
        {
            yield return 0;
        }
        LSLog.LogImportant("HAS JOINED ROOM - CREATING ENTITY");
        ColyseusManager.CreateNetworkedEntityWithTransform(new Vector2(-3.86f,14.39f), new Dictionary<string, object>() { ["prefab"] = "MultiPlayer"}, this, (entity) => {
            LSLog.LogImportant($"Network Entity Ready {entity.id}");
        });
    }
*/


    private void Update()
    {
        rb.MovePosition(rb.position + moveInput * speed * Time.deltaTime);

    }

/*
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
*/
//Function for changing stamina over time
    private void ChangeStamina(float rate)
    {
        currentStamina = Mathf.Clamp(currentStamina + rate, 0, maxStamina);
    }

    //Funciton for edible objects to change player stamina
    public void FoodStamina(int amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina); 
    }

    //Trigger Exits and Enters to set whether objects are near for interactions
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Fruit Tree") && i == 0) {
            fruitTreeNear = true;
            i++;
        } else if (other.gameObject.CompareTag("Tree") && i == 0) {
            treeNear = true;
            i++;
        } else if (other.gameObject.CompareTag("Player") && i == 0) {
            playerNear = true;
            Debug.Log("player near");
            i++;
        } else if (other.gameObject.CompareTag("Cave") && i == 0) {
            caveNear = true;
            i++;
        } 
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Fruit Tree") && i == 1) {
            fruitTreeNear = false;
            i = 0;
        } else if (other.gameObject.CompareTag("Tree") && i == 1) {
            treeNear = false;
            i = 0;
        } else if (other.gameObject.CompareTag("Player") && i == 1) {
            playerNear = false;
            i = 0 ;
        } else if (other.gameObject.CompareTag("Cave") && i == 1) {
            caveNear = false;
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
    public void OnMove(InputValue value) 
    {
        moveInput = value.Get<Vector2>();
/*
        if(!Mathf.Approximately(moveInput.x, 0.0f) || !Mathf.Approximately(moveInput.y, 0.0f))
        {
            lookDirection.Set(moveInput.x, moveInput.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", moveInput.magnitude);
*/  
    }

    //Functions for interacting with food objects (Fruit trees, caves, and other players)
    private void OnFoodAction()
    {
        RaycastHit2D hit;
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
        if (hit.collider.tag == "Fruit Tree" && fruitTreeNear && hit) {
            Debug.Log("Fruit Tree Clicked");
            //Invokes event for Fruit Tree to animate then send event back
            _gatherFruitEvent?.Invoke();
            animator.SetBool("Gather", true);
        } else if (hit.collider.tag == "Cave" && caveNear) {
            Cave cave = hit.collider.GetComponent<Cave>();
            cave.AddFood(food);
            DropOff();
            Debug.Log(cave.FoodCount);
        } else if (hit.collider.tag == "Player" && playerNear) {
            AddFood();
        }
    }
    public void StopGather() {
        //Triggers at the end of the gather animation
        Debug.Log("Gathering is finished");
        gameObject.GetComponent<Animator>().SetBool("Gather", false);
        AddFood();

    }    
    
    //Function for right clicking and observing a nearby object
    public void OnObserve()
    {
        RaycastHit2D hit;
        Debug.Log("there was a right click at " + Mouse.current.position.ReadValue());
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.Log(animator);
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

    public void AddFood()
    {
        food += 1;
        ChangedFood?.Invoke();
    }

    public void AddWood()
    {
        wood += 1;
        ChangedWood?.Invoke();
    }

    public void Robbed()
    {
        food -= 1;
        ChangedFood?.Invoke();
    }

    public void DropOff()
    {
        food -= food;
        ChangedFood?.Invoke();
    }

    public void UseWood()
    {
        wood -= wood;
        ChangedWood?.Invoke();
    }
}
