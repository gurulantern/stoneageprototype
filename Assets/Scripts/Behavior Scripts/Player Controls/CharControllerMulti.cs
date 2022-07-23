using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
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
    [SerializeField] GameObject _raycastCollider;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] PlayerControls _playerControls;
    [SerializeField] Camera _camera;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private UIHooks uiHooks; 
    [SerializeField] private Robbable robbable;
    [SerializeField] private GameObject _scareableField;

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
    public int fruit, meat, wood, seeds, fish;
    private int icon;
    private Vector2 moveInput;
    private string tagNear;
    private bool sleeping, observing, gathering, spending, tired, stealing, scaring;
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
    Rigidbody2D rb;
    Vector2 lookDirection = new Vector2(1,0);
    private Vector2 spawnPosition;
    public event Action<int> ChangedResource;
    

    #region Initializers
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

    public void InitializeObjectForRemote(NetworkedEntity entity)
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
        _raycastCollider.tag = "Other_Player";
        gameObject.tag = "Other_Player";
        teamIndex = GameController.Instance.GetTeamIndex(OwnerId);
        teamNumber = GameController.Instance.GetTeamNumber(teamIndex);
        SetTeam( entity,teamIndex, teamNumber);
    }

    public override void InitiView(NetworkedEntity entity)
    {
        base.InitiView(entity);
        robbable.remoteEntityID = entity.id;
        onPlayerActivated?.Invoke(this);
        teamIndex = GameController.Instance.GetTeamIndex(OwnerId);
        Debug.Log("Initializing view");

        //If we're in team death match, we need our team index
        if (!GameController.Instance.IsCoop && IsMine)
        {
            teamNumber = GameController.Instance.GetTeamNumber(teamIndex);
            SetTeam(entity, teamIndex, teamNumber);
            uiHooks.Initialize();
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
            _spriteRenderer.color = GameController.Instance.GetTeamColor(idx);
            SetStartPos(entity, teamIndex, teamNum);
            if (GameController.Instance._uiController.loadCover.activeInHierarchy)
            {
                GameController.Instance._uiController.loadCover.SetActive(false);
            }
        }
    }
    
    /// Sets player's home cave with array of refs and starting location based on spawn point refs from team number
    protected virtual void SetStartPos(NetworkedEntity entity, int idx, int spawnNum)
    {
        Cave myHomeCave = GameController.Instance.homeCaves[idx];
        myTransform.localPosition = myHomeCave.spawnPoints[spawnNum].spawn;
        entity.xPos = myTransform.localPosition.x;
        entity.yPos = myTransform.localPosition.y;
        if (IsMine) {
            myHomeCave.gameObject.tag = "Cave";
        }
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
        if (IsMine)// && GameController.Instance.gamePlaying)
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
    #endregion

    #region Inputs
    //function for sleeping
    public void OnSleep(InputAction.CallbackContext context)
    {
        if(!sleeping && !gathering && !spending && !observing && context.performed) {
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
    public void OnMove(InputAction.CallbackContext context) 
    {
        if (!sleeping && !observing && !gathering && !spending && !scaring && !stealing) {
        moveInput = context.ReadValue<Vector2>();

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
    public void OnInteractAction(InputAction.CallbackContext context)
    {
        Debug.Log("Gathering");
        RaycastHit2D hit;
        if (GameController.Instance.gamePlaying && !tired && !sleeping && !spending && !observing && context.performed) {
            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
   
            if (hit.collider.gameObject.GetComponentInParent<Robbable>() != null && hit.collider.gameObject.GetComponentInParent<CharControllerMulti>().proxyStates[0].sleep) {
                Robbable robbable = hit.collider.gameObject.GetComponentInParent<Robbable>(); 
                robbable.Steal(new Robbable.StoneAgeStealMessage()
                {
                    robber = Id,
                    isRFC = hit.collider.gameObject.CompareTag("Other_Player")
                });
                Debug.Log($"Attempting to steal from {robbable}");
            } else {
                currentGatherable = hit.collider.gameObject.GetComponent<Gatherable>();
                currentScorable = hit.collider.gameObject.GetComponent<Scorable>();
                if(currentGatherables.Contains(currentGatherable) && !animator.GetBool("Gather")) {
                    Debug.Log(currentGatherable + " clicked.");
                    if (currentGatherable.gameObject.tag == "Tree" && GameController.Instance.create == false) {
                        return;
                    } else {
                        currentGatherable.PlayerAttemptedUse(this);
                        Debug.Log("Gathering");
                    }
                } else if (currentScorables.Contains(currentScorable) && !animator.GetBool("Gather")) {
                    Debug.Log(currentScorable + " clicked");
                    switch(currentScorable.requiredResource) {
                        case "food":
                            if(state.fruit > 0 && state.meat > 0) {
                                icon = 2;
                                GameController.Instance.RegisterGather(this.Id, state.fruit.ToString(), state.meat.ToString(), teamIndex.ToString());
                                StartGather(false);
                            } else if (state.fruit > 0) {
                                icon = 0;
                                GameController.Instance.RegisterGather(this.Id, state.fruit.ToString(), "0", teamIndex.ToString());
                                StartGather(false);
                            } else if (state.meat > 0) {
                                icon = 1;
                                GameController.Instance.RegisterGather(this.Id, "0", state.meat.ToString(), teamIndex.ToString());
                                StartGather(false);
                            } else {
                                icon = -1;
                            }
                            break;
                        case "wood":
                            if (state.wood > 0) {
                                icon = 3;
                                currentScorable.PlayerAttemptedUse(this, (int)state.wood);
                                StartGather(false);
                            } else {
                                icon = -1;
                            }
                            break;
                        case "seeds":
                            if (state.seeds > 0) {
                                icon = 4;
                                currentScorable.PlayerAttemptedUse(this, (int)state.seeds);
                                StartGather(false);
                            } else {
                                icon = -1;
                            }
                            break;
                    }
                }
            }
        }
    }

    //If attempted use is successful this function will fire
    public void StartGather(bool gatherOrSpend)
    {
        if (gatherOrSpend) {
            if (!stealing) {
                switch(currentGatherable.gameObject.tag) {
                        case "Fruit_Tree":
                            if (GameController.Instance.create) {
                                icon = 3;
                            } else {
                                icon = 0;    
                            }
                            break;
                        case "Aurochs":
                            icon = 1;
                            break;
                        case "Tree":
                            icon = 2;
                            break;
                        case "Fishing_Spot":
                            icon = 4;
                            break;
                }
            }
            gathering = true;
            animator.SetBool("Gather", true);
            Debug.Log("Gathering with icon " + icon);
            _gatherIcons[icon].gameObject.SetActive(true);
        } else {
            spending = true;
            animator.SetBool("Gather", true);
            _spendIcons[icon].gameObject.SetActive(true);
        }
    }


    //The function to stop the gather aciton called when animation has finished;
    public void StopGather() {
        //Triggers at the end of the gather animation
        if (gathering) {
            Debug.Log("Gathering is finished");
            _gatherIcons[icon].gameObject.SetActive(false);
            AddResource(icon);
            animator.SetBool("Gather", false);
            gathering = false;
            stealing = false;
        } else if (spending) {
            Debug.Log("Spending is finished");
            _spendIcons[icon].gameObject.SetActive(false);
            SubtractResource(icon);
            animator.SetBool("Gather", false);
            spending = false;
        }
    }
    
    //Function for right clicking and observing a nearby object
    public void OnObserve(InputAction.CallbackContext context)
    {
        RaycastHit2D hit;
        Debug.Log("there was a right click at " + Mouse.current.position.ReadValue());
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        hit = Physics2D.GetRayIntersection(ray, 20, _layerMask);
        tagNear = hit.collider.gameObject.tag;
        if(tagNear != null && !observing && !gathering && !tired && !stealing && !spending && context.performed) {
            observing = true;
            animator.SetBool("Observe", true);
            GameController.Instance.RegisterObserve(this.Id, hit.collider.gameObject.tag, teamIndex.ToString());
            Debug.Log($"Player observes {hit.collider.gameObject.tag}");
        }
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
        } else if (icon == 1) {
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
            state.fruit -= state.fruit;
            fruit = (int)state.fruit;
        } else if (icon == 1) {
                state.meat -= state.meat;
                meat = (int)state.meat;
        } else if (icon == 2) {
                state.fruit -= state.fruit;
                fruit = (int)state.fruit;
                state.meat -= state.meat;
                meat = (int)state.meat;
        } else if (icon == 3 && GameController.Instance.create) {
                state.wood -= state.wood;
                wood = (int)state.wood;
        }  else if (icon == 4 && GameController.Instance.create) {
                state.seeds -= state.seeds;
                seeds = (int)state.seeds;
        } else if (icon == 5 && GameController.Instance.create) {
            state.wood -= state.wood;
            wood = (int)state.wood;
            state.seeds -= state.seeds;
            seeds = (int)state.seeds; 
        }
        ChangedResource?.Invoke(icon);
        Debug.Log($"Fruit = {fruit}, Meat = {meat}, Wood = {wood}, Seeds = {seeds}");
    }

    public void OnScare(InputAction.CallbackContext context)
    {
        if (context.performed) {
            scaring = true;
            _scareableField.GetComponent<CircleCollider2D>().enabled = true;
        }

        if (context.canceled) {
            scaring = false;
            _scareableField.GetComponent<CircleCollider2D>().enabled = false;
        } 
    }

    #endregion 

    #region Remote Function Calls
    public void Robbed(string robberID) 
    {
        ColyseusManager.RFC(this, "RobbedRFC", new object[]{ Id, robberID }, RFCTargets.OTHERS);
    }

    public void RobbedRFC(string entityID, string robberID)
    {
        if (entityID.Equals(Id))
        {
            int type = PickGoods();
            if (ColyseusManager.Instance.HasEntityView(robberID) == false) {
                //ColyseusManager.AddEntityView(robberID, )
            }
            Robbable robbable = ColyseusManager.Instance.GetEntityView(robberID).gameObject.GetComponent<Robbable>();
            robbable.Give(new Robbable.StoneAgeGiveMessage()
            {
                giver = OwnerId,
                type = type,
                isRFC = robbable.gameObject.CompareTag("Other_Player")
            });
            Debug.Log($"Attempting to give to {robbable}");
        }
    }

    public int PickGoods()
    {
        int icon = Mathf.RoundToInt(UnityEngine.Random.Range(0.0f, 2.0f));
        switch (icon) {
            case 0:
                if (fruit > 0) {
                    state.fruit -= 1f;
                    fruit = (int)state.fruit;
                    ChangedResource?.Invoke(icon);
                    return icon;
                } else {
                    return 4;
                }
            case 1:
                if (meat > 0) {
                    state.meat -= 1f;
                    meat = (int)state.meat;
                    ChangedResource?.Invoke(icon);
                    return icon;
                } else {
                    return 4;
                }
            case 2:
                if (wood > 0) {
                    state.wood -= 1f;
                    wood = (int)state.wood;
                    ChangedResource?.Invoke(icon);
                    return icon;
                } else {
                    return 4;
                }
            default:
                return 4;
        }
    }
    

    public void Receive(string giverID, int type)
    {
        ColyseusManager.RFC(this, "ReceiveRFC", new object[]{ Id, giverID, type}, RFCTargets.OTHERS);
    }

    public void ReceiveRFC(string entityID, string giverId,  int type)
    {
        if (entityID.Equals(Id))
        {
            Debug.Log("Ready to Receive!");
            icon = type;
            stealing = true;
            switch (type) {
                case 0:
                    StartGather(stealing);
                    break;
                case 1:
                    StartGather(stealing);
                    break;
                case 2:
                    StartGather(stealing);
                    break;
                case 4:
                    StartGather(stealing);
                    break;
                default:
                    break;
            }
        }
    }

    public void EntityNearInteractable(Interactable interactable)
    {
        if (interactable.GetComponent<Gatherable>()) {
            currentGatherables.Add(interactable.GetComponent<Gatherable>());
            //Debug.Log($"Added {interactable.GetComponent<Gatherable>()}");
        } else if (interactable.GetComponent<Scorable>()) {
            currentScorables.Add(interactable.GetComponent<Scorable>());
            //Debug.Log($"{currentScorables}");
        }
    }

    public void EntityLeftInteractable(Interactable interactable)
    {
        if (interactable.GetComponent<Gatherable>()) {
            currentGatherables.Remove(interactable.GetComponent<Gatherable>());
            //Debug.Log($"Removing {interactable.GetComponent<Gatherable>()}");
        } else if (interactable.GetComponent<Scorable>()) {
            currentScorables.Remove(interactable.GetComponent<Scorable>());
            //Debug.Log($"{currentScorables}");
        }
    }



    public void Scared(Vector2 scarerPosition)
    {

    }

    public void ScaredRFC()
    {

    }
    #endregion
}