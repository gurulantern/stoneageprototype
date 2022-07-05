using UnityEngine;
using UnityEngine.InputSystem;
using Colyseus;
using System;
using System.Collections;
using System.Collections.Generic;
using LucidSightTools;

public class CharControllerMulti : NetworkedEntityView
{
    public delegate void OnPlayerActivated(CharControllerMulti playerController);
    public static event OnPlayerActivated onPlayerActivated;
    public delegate void OnPlayerDeactivated(CharControllerMulti playerController);
    public static event OnPlayerDeactivated onPlayerDeactivated;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] PlayerControls _playerControls;
    [SerializeField] Camera _camera;
    [SerializeField] GameEvent _gatherFruitEvent;
    [SerializeField] GameEvent _gatherMeatEvent;
    [SerializeField] GameEvent _gatherWoodEvent;
    [SerializeField] GameEvent _dropOffEvent;
    [SerializeField] GameEvent _observeDone;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private UIHooks uiHooks; 

    [SerializeField] private SpriteRenderer[] _gatherIcons;
    private ICollection entities;
    public PlayerStats _playerStats;
    private NetworkedEntity updatedEntity;
    public float maxStamina, currentStamina, speed, tiredSpeed, tireLimit, tireRate, restoreRate;
    public int food, wood, seeds;
    private int icon;
    private Vector2 moveInput;
    private bool treeNear, fruitTreeNear, aurochsNear, playerNear, caveNear;
    private string tagNear;
    private bool sleeping, observing, gathering, tired;
    //Iterator variable for debugging Trigger Enter and Exit
    private int i = 0;
    [SerializeField]
    private int teamIndex = -1;
    [SerializeField]
    private int teamNumber;
    public int TeamIndex
    {
        get 
        { 
            return teamIndex; 
        }
    }
    public Color[] teamColors;
    Rigidbody2D rb;
    Vector2 lookDirection = new Vector2(1,0);
    private Vector2 spawnPosition;
    public event Action<int> ChangedResource;
    
    protected override void Awake() {
        base.Awake();
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
        GameController.onUpdateClientTeam += OnTeamUpdated;    
    }

    private void OnDisable() 
    {
        GameController.onUpdateClientTeam -= OnTeamUpdated;
        onPlayerDeactivated?.Invoke(this);    
    }

    protected override void Start()
    {
        base.Start();
        sleeping = false;

    }

    private void OnTeamUpdated(int team, string id)
    {
        //If we're in team death match, we need our team index
        if (!GameController.Instance.IsCoop && id.Equals(OwnerId))
        {
            teamNumber = GameController.Instance.GetTeamNumber(team);
            SetTeam(state, team, teamNumber);
            Debug.Log($"Your team is {team}");
        }
    }

    public void InitializeObjectForRemote()
    {
        //Arrange this prefab to work well as a remote view but disabling certain scripts (rather than have a unique second prefab)
        if (TryGetComponent(out PlayerInput playerInput))
        {
            Destroy(playerInput);
        }
        if (TryGetComponent(out UIHooks uiHooks))
        {
            Destroy(uiHooks);
        }
        gameObject.tag = "OtherPlayer";

        /*
        if (TryGetComponent(out PlayerSpaceshipInputBehaviour inputBehaviour))
        {
            Destroy(inputBehaviour);
        }
        if (TryGetComponent(out PlayerCameraController cameraController))
        {
            Destroy(cameraController);
        }

        gameObject.tag = "OtherShip";
        gameObject.layer = 11;  //This is "OtherShip" in the physics layer
        */
    }

    public override void InitiView(NetworkedEntity entity)
    {
        base.InitiView(entity);
        onPlayerActivated?.Invoke(this);
        teamIndex = GameController.Instance.GetTeamIndex(OwnerId);
        Debug.Log("Initializing view");

        //If we're in team death match, we need our team index
        if (!GameController.Instance.IsCoop)
        {
            teamNumber = GameController.Instance.GetTeamNumber(teamIndex);
            SetTeam(entity, teamIndex, teamNumber);
        }

        if (IsMine)
        {
            spawnPosition = transform.position;
        }
    }

    private void SetTeam(NetworkedEntity entity, int idx, int teamNum)
    {
        teamIndex = idx;
        if (teamIndex >= 0)
        {
            SetStartPos(entity, teamIndex, teamNum);
            SetPlayerColor(teamColors[teamIndex]);
            if (GameController.Instance.uiController.loadCover.activeInHierarchy)
            {
                GameController.Instance.uiController.loadCover.SetActive(false);
            }
        }
    }

    public void SetPlayerColor(Color color)
    {
        _spriteRenderer.color = color;
    }
    /// Sets player's spawn point using home cave refs and spawn point refs based on team number
    protected virtual void SetStartPos(NetworkedEntity entity, int idx, int spawnNum)
    {
        myTransform.localPosition = GameController.Instance.homeCaves[idx].spawnPoints[spawnNum].spawn;
        entity.xPos = myTransform.localPosition.x;
        entity.yPos = myTransform.localPosition.y;
    }

    public void SetWakeState(NetworkedEntity entity, bool wakeState) 
    {

    }

    public override void OnEntityRemoved()
    {
        base.OnEntityRemoved();
        LSLog.LogImportant("REMOVING ENTITY", LSLog.LogColor.lime);
        Destroy(this.gameObject);
    }
    protected override void Update()
    {
        base.Update(); 
        if (!IsMine)
        {
            if(!Mathf.Approximately(localPositionDelta.x, 0.0f) || !Mathf.Approximately(localPositionDelta.y, 0.0f))
            {
                lookDirection.Set(localPositionDelta.x, localPositionDelta.y);
                lookDirection.Normalize();
            }

            animator.SetFloat("Look X", lookDirection.x);
            animator.SetFloat("Look Y", lookDirection.y);
            animator.SetFloat("Speed", localPositionDelta.magnitude * 100);
        }
    }
    
    /// Animator code for players that are not mine
    protected override void ProcessViewSync()
    {
        base.ProcessViewSync();
        animator.SetBool("Sleep", proxyStates[0].sleep);
        animator.SetBool("Tired", proxyStates[0].tired);
        animator.SetBool("Awake", proxyStates[0].wake);
        animator.SetBool("Observe", proxyStates[0].observe);
        animator.SetBool("Gather", proxyStates[0].gather);
    }

    void FixedUpdate()
    {
        if (IsMine) //&& GameController.Instance.gamePlaying)
        {
            if (currentStamina == maxStamina && sleeping == true) {
                //When stamina is full after sleeping call Wake
                Wake();
            } else if (sleeping == true) {
                //When sleep is true and stamina is not full, restore stamina
                ChangeStamina(restoreRate);
            } else if (animator.GetBool("Observe") || animator.GetBool("Gather")) {    
                ChangeStamina(-tireRate);
            } else if (sleeping == false && currentStamina <= tireLimit) {
                //When Awake and stamina is under tire limit, enter tired animation and slow down
                animator.SetBool("Tired", true);
                ChangeStamina(-tireRate);
                rb.MovePosition(rb.position + moveInput * tiredSpeed * Time.fixedDeltaTime);
            } else if (sleeping == false) {
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
        if (uiHooks && IsMine)
        {
            currentStamina = Mathf.Clamp(currentStamina + rate, 0, maxStamina);
        }
    }

    //Function for edible objects to change player stamina
    public void FoodStamina(int amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina); 
    }

    //Trigger Exits and Enters to set whether objects are near for interactions
    private void OnTriggerEnter2D(Collider2D other) {
        if (other && i == 0) {
            tagNear = other.gameObject.tag;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other && i == 1) {
            tagNear = "";
            i = 0;
        }
    }

    //function for sleeping
    private void OnSleep()
    {
        if(sleeping == false) {
            sleeping = true;
            animator.SetBool("Tired", false);
            animator.SetBool("Awake", false);
            animator.SetBool("Sleep", true);
        }
    }
    //function for waking up
    void Wake()
    {
        animator.SetBool("Sleep", false);
        animator.SetBool("Tired", false);
        animator.SetBool("Awake", true);
        sleeping = false;

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
        string tag;
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
        tag = hit.collider.tag;
        if(tag == tagNear) {
            Debug.Log(tag + " clicked.");
            switch(tag) {
                case "Fruit Tree":
                    _gatherFruitEvent?.Invoke();
                    icon = 0;
                    animator.SetBool("Gather", true);
                    break;
                case "Tree":
                    _gatherWoodEvent?.Invoke();
                    if (GameController.Instance.create) {    
                        animator.SetBool("Gather", true);
                    }
                    icon = 1;
                    break;
                case "Aurochs":
                    _gatherMeatEvent?.Invoke();
                    animator.SetBool("Gather", true);
                    icon = 2;
                    break;
                case "OtherPlayer":
                    _gatherFruitEvent?.Invoke();
                    icon = 0;
                    if (GameController.Instance.steal ) {
                        animator.SetBool("Gather", true);
                    }
                    break;
                case "Cave":
                    _gatherFruitEvent?.Invoke();
                    icon = 0;
                    break;
                default:
                    Debug.Log("No gather event called.");
                    icon = -1;
                    break;
            }
            animator.SetBool("Gather", true);
            _gatherIcons[icon].gameObject.SetActive(true);

        }
    }

    //The function to stop the gather aciton called when animation has finished;
    public void StopGather() {
        //Triggers at the end of the gather animation
        Debug.Log("Gathering is finished");
        _gatherIcons[icon].gameObject.SetActive(false);
        animator.SetBool("Gather", false);
        AddResource(icon);
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
        } else if (hit.collider.tag == "Tree" && treeNear) {
            animator.SetBool("Observe", true);
            Debug.Log("Player observes");
        } else if (hit.collider.tag == "OtherPlayer" && playerNear) {
            animator.SetBool("Observe", true);
            Debug.Log("Player observes");
        } else if (hit.collider.tag == "Aurochs" && aurochsNear) {
            animator.SetBool("Observe", true);
        }
    }

    // function to notify when Obsering is done, called when animation is finished
    public void StopObserve()
    {
        animator.SetBool("Observe", false);
        _observeDone?.Invoke();
    }

    public void AddResource(int icon)
    {
        if (icon == 0) {
            state.food += 1f;
            food = (int)state.food;
            if (GameController.Instance.create) {
                state.seeds +=  5f;
                seeds = (int)state.seeds;
            }
        } else if (icon == 1 && GameController.Instance.create) {
                state.wood += 1f;
                wood = (int)state.wood;
        } else if (icon == 2) {
                state.food += 10f;
                food = (int)state.food;
        } 
        ChangedResource?.Invoke(icon);
        Debug.Log($"Food = {food}, Wood = {wood}, Seeds = {seeds}");
    }

    public void Robbed()
    {
        state.food -= 1f;
        food = (int)state.food;
        ChangedResource?.Invoke(0);
        Debug.Log(state.seeds + "+" + seeds);

    }

    public void DropOff()
    {
        state.food -= state.food;
        ChangedResource?.Invoke(0);
        Debug.Log(state.food + "+" + food);
    }

    public void UseWood(int woodUsed)
    {
        state.wood -= (float)woodUsed;
        wood = (int)state.wood;
        ChangedResource?.Invoke(1);
        Debug.Log(state.wood + "+" + wood);
    }
}
