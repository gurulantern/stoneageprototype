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
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private UIHooks uiHooks; 

    [SerializeField] private SpriteRenderer[] _gatherIcons;
    [SerializeField] private SpriteRenderer[] _spendIcons;

    private List<Gatherable> currentGatherables;
    private List<Scorable> currentScorables;
    private Gatherable currentGatherable;
    private Scorable currentScorable;
    private ICollection entities;
    public PlayerStats _playerStats;
    private NetworkedEntity updatedEntity;
    public float maxStamina, currentStamina, speed, tiredSpeed, tireLimit, tireRate, restoreRate;
    public int fruit, meat, wood, seeds;
    private int icon;
    private Vector2 moveInput;
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
        currentGatherables = new List<Gatherable>();
        currentScorables = new List<Scorable>();
        _playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        maxStamina = _playerStats.MaxStamina;
        currentStamina = _playerStats.MaxStamina;
        speed = _playerStats.Speed;
        tiredSpeed = _playerStats.TiredSpeed;
        tireLimit = _playerStats.TireLimit;
        tireRate = _playerStats.TireRate;
        restoreRate = _playerStats.RestoreRate;
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
        gameObject.tag = "Other_Player";
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
            SetPlayerColor(teamColors[teamIndex]);
            SetStartPos(entity, teamIndex, teamNum);
            if (GameController.Instance._uiController.loadCover.activeInHierarchy)
            {
                GameController.Instance._uiController.loadCover.SetActive(false);
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

    public void ChangeStamina(float rate)
    {
        if (uiHooks && IsMine)
        {
            currentStamina = Mathf.Clamp(currentStamina + rate, 0, maxStamina);
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
        if (!sleeping && !observing && !gathering) {
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
    }

    /// Functions for interacting with food objects (Fruit trees, caves, and other players)
    /// Checks if the gatherable has a resource requirement and if the current player has the resource
    private void OnFoodAction()
    {
        RaycastHit2D hit;
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
        if (hit.collider.gameObject.tag == "OtherPlayer" && hit.collider.gameObject.GetComponent<CharControllerMulti>().sleeping) {
            
        } else {
            currentGatherable = hit.collider.gameObject.GetComponent<Gatherable>();
            currentScorable = hit.collider.gameObject.GetComponent<Scorable>();
            if(currentGatherables.Contains(currentGatherable) && !animator.GetBool("Gather")) {
                Debug.Log(currentGatherable + " clicked.");
                currentGatherable.PlayerAttemptedUse(this);
            } else if (currentScorables.Contains(currentScorable) && !animator.GetBool("Gather")) {
                Debug.Log(currentScorable + " clicked");
                switch(currentScorable.requiredResource) {
                    case "food":
                        if(state.fruit > 0 && state.meat > 0) {
                            icon = 2;
                        } else if (state.fruit > 0) {
                            icon = 0;
                        } else if (state.meat > 0) {
                            icon = 1;
                        }
                        break;
                    case "wood":
                        if(state.wood > 0) {
                            icon = 3;
                        }
                        break;
                    case "wood + seeds":
                        if(state.wood > 0  && state.seeds > 0) {
                            icon = 5;
                        } else if (state.wood > 0) {
                            icon = 3;
                        } else if (state.seeds > 0) {
                            icon = 4;
                        }
                        break;
                }
                currentScorable.PlayerAttemptedUse(this);
            }
        }
    }

    //If attempted use is successful this function will fire
    public void StartGather()
    {
        switch(currentGatherable.gameObject.tag) {
                case "Fruit_Tree":
                    if (GameController.Instance.create) {
                        icon = 3;
                    } else {
                        icon = 0;    
                    }
                    break;
                case "Live_Aurochs":
                    icon = 1;
                    break;
                case "Dead_Aurochs":
                    icon = 1;
                    break;
                case "Tree":
                    if (GameController.Instance.create) {
                        icon = 2;
                    }
                    break;
                case "Fish":
                    icon = 4;
                    break;
            }
        gathering = true;
        animator.SetBool("Gather", true);
        _gatherIcons[icon].gameObject.SetActive(true);
    }


    //The function to stop the gather aciton called when animation has finished;
    public void StopGather() {
        //Triggers at the end of the gather animation
        Debug.Log("Gathering is finished");
        _gatherIcons[icon].gameObject.SetActive(false);
        animator.SetBool("Gather", false);
        gathering = false;
        AddResource(icon);
    }

    public void StartSpend() {
        switch(currentGatherable.gameObject.tag) {
                case "Cave":

                    break;
                case "Farm_1":

                    break;
                case "Farm_2":

                    break;
                case "Aurochs_Pen":

                    break;
                case "Fish_Trap":

                    break;
            }
        gathering = true;
        animator.SetBool("Gather", true);
        _spendIcons[icon].gameObject.SetActive(true);
    }    

    public void StopSpend() {
        //Triggers at the end of the gather animation
        Debug.Log("Spending is finished");
        _spendIcons[icon].gameObject.SetActive(false);
        animator.SetBool("Gather", false);
        gathering = false;
        SubtractResource(icon);
    }
    
    //Function for right clicking and observing a nearby object
    public void OnObserve()
    {
        RaycastHit2D hit;
        Debug.Log("there was a right click at " + Mouse.current.position.ReadValue());
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
        tagNear = hit.collider.gameObject.tag;
        if(tagNear != null) {
            observing = true;
            animator.SetBool("Observe", true);
            Debug.Log("Player observes");
        }
    }

    public void RegisterObserved()
    {
        
    }

    // function to notify when Obsering is done, called when animation is finished
    public void StopObserve()
    {
        animator.SetBool("Observe", false);
        observing = false;
    }

    public void AddResource(int icon)
    {
        if (icon == 0) {
            state.fruit += 1f;
            fruit = (int)state.fruit;
        } else if (icon == 1  || icon == 4) {
                state.meat += 1f;
                meat = (int)state.meat;
        } else if (icon == 2 && GameController.Instance.create) {
                state.wood += 1f;
                wood = (int)state.wood;
        }  else if (icon == 3 && GameController.Instance.create) {
            state.fruit += 1f;
            fruit = (int)state.fruit;
            state.seeds += 5f;
            seeds = (int)state.seeds;
        }
        ChangedResource?.Invoke(icon);
        Debug.Log($"Fruit = {fruit}, Meat = {meat}, Wood = {wood}, Seeds = {seeds}");
    }

    public void SubtractResource(int icon)
    {
        if (icon == 0) {
            state.fruit += 1f;
            fruit = (int)state.fruit;
        } else if (icon == 1  || icon == 4) {
                state.meat += 1f;
                meat = (int)state.meat;
        } else if (icon == 2 && GameController.Instance.create) {
                state.wood += 1f;
                wood = (int)state.wood;
        }  else if (icon == 3 && GameController.Instance.create) {
            state.fruit += 1f;
            fruit = (int)state.fruit;
            state.seeds += 5f;
            seeds = (int)state.seeds;
        }
        ChangedResource?.Invoke(icon);
        Debug.Log($"Fruit = {fruit}, Meat = {meat}, Wood = {wood}, Seeds = {seeds}");
    }

    public void Robbed(string robberID) 
    {
        //ColyseusManager.RFC(this, "RobbedRFC", );
    }

    public void RobbedRFC()
    {

    }

    public void Give(string receiverID)
    {

    }

    public void GiveRFC(string receiverID)
    {

    }

    public void DropOff()
    {
        state.fruit -= state.fruit;
        state.meat -= state.meat;
        ChangedResource?.Invoke(0);
        Debug.Log(state.fruit + "+" + fruit);
    }

    public void UseWood(int woodUsed)
    {
        state.wood -= (float)woodUsed;
        wood = (int)state.wood;
        ChangedResource?.Invoke(1);
        Debug.Log(state.wood + "+" + wood);
    }

    public void EntityNearInteractable(Interactable interactable)
    {
        if (interactable.GetComponent<Gatherable>()) {
            currentGatherables.Add(interactable.GetComponent<Gatherable>());
            Debug.Log($"{currentGatherables}");
        } else if (interactable.GetComponent<Scorable>()) {
            currentScorables.Add(interactable.GetComponent<Scorable>());
            Debug.Log($"{currentScorables}");
        }
    }

    public void EntityLeftInteractable(Interactable interactable)
    {
        if (interactable.GetComponent<Gatherable>()) {
            currentGatherables.Remove(interactable.GetComponent<Gatherable>());
            Debug.Log($"{currentGatherables}");
        } else if (interactable.GetComponent<Scorable>()) {
            currentScorables.Remove(interactable.GetComponent<Scorable>());
            Debug.Log($"{currentScorables}");
        }
    }
}
