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
    [SerializeField] GameEvent _dropOffEvent;
    [SerializeField] GameEvent _observeDone;
    [SerializeField] private LayerMask _layerMask;
    private ICollection entities;
    public PlayerStats _playerStats;
    private NetworkedEntity updatedEntity;
    public float maxStamina, currentStamina, speed, tiredSpeed, tireLimit, tireRate, restoreRate;
    public int food, wood;
    private Vector2 moveInput;
    private bool treeNear, fruitTreeNear, playerNear, caveNear;
    private bool sleep, observing, gathering, tired;
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
    public Renderer[] playerRenderers;
    Rigidbody2D rb;
    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);
    private Vector2 spawnPosition;
    public event Action ChangedFood;
    public event Action ChangedWood;
    
    protected override void Awake() {
        base.Awake();
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
        sleep = false;

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
        /*
        if (TryGetComponent(out PlayerSpaceshipInputBehaviour inputBehaviour))
        {
            Destroy(inputBehaviour);
        }
        if (TryGetComponent(out PlayerCameraController cameraController))
        {
            Destroy(cameraController);
        }
        if (TryGetComponent(out UIHooks uiHooks))
        {
            Destroy(uiHooks);
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
    
    protected override void ProcessViewSync()
    {
        base.ProcessViewSync();
    }

    void FixedUpdate()
    {
        if (IsMine)
        {
            rb.MovePosition(rb.position + moveInput * speed * Time.deltaTime);
        }

        /*
        if (IsMine && GameController.instance.gamePlaying)
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
        */
    }
//Function for changing stamina over time

    public void PositionAtSpawn()
    {
        transform.position = GetSpawnPosition();
    }

    private Vector2 GetSpawnPosition()
    {
        if (GameController.Instance.IsCoop)
        {
            Vector2 randomPt = UnityEngine.Random.insideUnitSphere * 10;
            randomPt.y = 0;
            return spawnPosition + randomPt;
        }

        return GameController.Instance.GetSpawnPoint(TeamIndex);
    }
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
