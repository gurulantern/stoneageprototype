using System;
using Colyseus;
using LucidSightTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using StoneAge.Controllers;

public class NetworkedEntityView : StoneColyseusNetworkedEntityView
{
    public string OwnerId { get; private set; }
    public int RefId { get; private set; }
    public bool IsEntityOwner { get; private set; }
    public bool IsMine { get; private set; }
    public bool HasInit { get; private set; }
    public string UserName { get; private set; }
    public bool autoInitEntity = false;
    public string prefabName;
    protected Animator animator;

    //public configs
    [HeaderAttribute("Position Sync")]
    public float positionLerpSpeed = 5f;
    //[HeaderAttribute("Rotation Sync")]
    public float snapIfAngleIsGreater = 100f;
    //public float rotationLerpSpeed = 5f;

    public float stateSyncUpdateRateMs = 100f;

    //Movement Sync
    public double interpolationBackTimeMs = 200f;
    public double extrapolationLimitMs = 500f;
    public float maxSpeedDeltaSqr = 9f;

    public bool syncLocalPosition = true;
    //public bool syncLocalRotation = true;
    public bool checkForSpeedHacks = false;

    [SerializeField]
    protected NetworkedEntity state;
    protected NetworkedEntity previousState;
    protected NetworkedEntity localUpdatedState;

    //Last time state was updated here was its hash
    protected double lastStateTimestamp;
    private float counterStateSyncUpdateRate = 0f;
    private bool lerpPosition = false;
    private long lastFullSync = 0;

    // Clients store twenty states with "playback" information from the server. This
    // array contains the official state of this object at different times according to
    // the server.
    [SerializeField]
    protected EntityState[] proxyStates = new EntityState[20];

    // Keep track of what slots are used
    protected int proxyStateCount;

    /// Cached transform
    protected Transform myTransform;
    /// The change in position in the most recent frame. Applies
    /// to all sessions including the owner
    public Vector3 LocalPositionDelta
    {
        get
        {
            return localPositionDelta;
        }
    }
    protected Vector3 localPositionDelta;

    /// The position of this transform in the previous frame
    protected Vector3 prevLocalPosition;

    /// Synchronized object state
    [System.Serializable]
    protected struct EntityState
    {
        public double timestamp;
        public Vector2 pos;
        public Vector2 vel;
        public Quaternion rot;
        public bool sleep;
        public bool tired;
        public bool wake;
        public bool observe;
        public bool gather;
        public int fruit;
        public int meat;
        public int wood;
        public int seeds;
        public int observePoints;
        public Colyseus.Schema.MapSchema<string> attributes;
    }

    protected virtual void Awake()
    {
        myTransform = transform;
        animator = gameObject.GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        if (autoInitEntity)
            InitiWithServer();
    }

    public void InitiWithServer()
    {
        StartCoroutine("Co_InitiWithServer");
    }

    IEnumerator Co_InitiWithServer()
    {
        while (ColyseusManager.Instance.isInRoom == false)
        {
            yield return 0;
        }

        if (autoInitEntity && HasInit == false && !string.IsNullOrEmpty(prefabName))
        {
            UserName = ColyseusManager.Instance.UserName;
            Debug.Log($"Instantiating a player with username - {UserName}");
            ColyseusManager.CreateNetworkedEntity(prefabName, new Dictionary<string, object> { ["userName"] = UserName }, this);
        }
        else if (autoInitEntity && HasInit == false && string.IsNullOrEmpty(prefabName))
        {
            LSLog.LogError("Prefab Name / Location Must be set");
        }
    }


    public virtual void InitiView(NetworkedEntity entity)
    {
        try
        {
            state = entity;
            IsMine = ColyseusManager.Instance.CurrentUser != null && string.Equals(ColyseusManager.Instance.CurrentUser.sessionId, state.ownerId);
            state.attributes.OnChange += Attributes_OnChange;
            state.OnChange += Entity_State_OnChange;

            if (state.attributes.TryGetValue("userName", out string userName))
            {
                //Debug.Log("Setting username as object name");
                UserName = userName;
                gameObject.name = UserName;
            }

            OwnerId = state.ownerId;
            Id = state.id;
            RefId = state.__refId;

            //Syncs Transform on Initi
            //SetStateStartPos();

            //set my transform
            if (myTransform == null) myTransform = transform;

            //Save lastLoc
            prevLocalPosition = myTransform.localPosition;

            HasInit = true;
        }
        catch (System.Exception e)
        {
            LSLog.LogError($"Error: {e.Message + e.StackTrace}");
        }
    }

    public virtual void OnEntityRemoved()
    {
        // Entity removed from room state;
    }

    virtual protected void Entity_State_OnChange(List<Colyseus.Schema.DataChange> changes)
    {
        // Only record state change has been updated locally
        lastStateTimestamp = state.timestamp;

        //If not mine Sync
        if (!IsMine)
        {
            UpdateViewFromState();
        }
    }

    virtual protected void Attributes_OnChange(string value, string key)
    {
        //LSLog.LogImportant($"Attribute Update {key}: {value}");
    }

    //Deserialize the state and updates the local view
    virtual protected void UpdateViewFromState()
    {

        // Network player, receive data
        Vector2 pos = new Vector2((float)state.xPos, (float)state.yPos);
        Vector2 velocity = new Vector2((float)state.xVel, (float)state.yVel);
        bool sleep = (bool)state.sleep;
        bool tired = (bool)state.tired;
        bool wake = (bool)state.wake;
        bool observe = (bool)state.observe;
        bool gather = (bool)state.gather;
        int fruit = (int)state.fruit;
        int meat = (int)state.meat;
        int wood = (int)state.wood;
        int seeds = (int)state.seeds;
        int observePoints = (int)state.observePoints;

        // If we're ignoring position data from the owning session, then use our own values. This
        // should only happen in special cases
        if (!syncLocalPosition)
        {
            pos = myTransform.localPosition;
            velocity = localPositionDelta;
        }

        // Check for speed hacks
        if (GameController.Instance.gamePlaying && checkForSpeedHacks && proxyStates.Length > 0)
        {
            Vector2 delta = pos - proxyStates[0].pos;
            if (delta.sqrMagnitude > maxSpeedDeltaSqr)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Speed hack detected. Throttling velocity of " + delta.magnitude);
                pos = proxyStates[0].pos + delta.normalized * Mathf.Sqrt(maxSpeedDeltaSqr);
#endif
            }
        }

        // Shift the buffer sideways, deleting state 20
        for (int i = proxyStates.Length - 1; i >= 1; i--)
        {
            proxyStates[i] = proxyStates[i - 1];
        }

        // Record current state in slot 0
        EntityState newState = new EntityState() { timestamp = state.timestamp }; //Make sure timestamp is in ms
                                                                                    //newState.timestamp = state.timestamp;

        newState.pos = pos;
        newState.vel = velocity;
        newState.sleep = sleep;
        newState.tired = tired;
        newState.wake = wake;
        newState.observe = observe;
        newState.gather = gather;
        newState.fruit = fruit;
        newState.meat = meat;
        newState.wood = wood;
        newState.seeds = seeds;
        newState.observePoints = observePoints;
        newState.attributes = state.Clone().attributes;
        proxyStates[0] = newState;


        // Update used slot count, however never exceed the buffer size
        // Slots aren't actually freed so this just makes sure the buffer is
        // filled up and that uninitalized slots aren't used.
        proxyStateCount = Mathf.Min(proxyStateCount + 1, proxyStates.Length);

        // Check if states are in order
        if (proxyStates[0].timestamp < proxyStates[1].timestamp)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Timestamp inconsistent: " + proxyStates[0].timestamp + " should be greater than " + proxyStates[1].timestamp);
#endif
        }

    }

    //Take Serializes changes in the tranform and animator and pass those changes to the server
    protected virtual void UpdateStateFromView()
    {

        previousState = state.Clone();

        //Copy Transform to State (round position to fix floating point issues with state compare)
        state.xPos = (float)System.Math.Round((decimal)transform.localPosition.x, 4);
        state.yPos = (float)System.Math.Round((decimal)transform.localPosition.y, 4);

        state.wRot = transform.localRotation.w;

        state.xScale = transform.localScale.x;
        state.yScale = transform.localScale.y;

        state.xVel = localPositionDelta.x;
        state.yVel = localPositionDelta.y;

        state.sleep = animator.GetBool("Sleep");
        state.tired = animator.GetBool("Tired");
        state.wake = animator.GetBool("Awake");
        state.observe = animator.GetBool("Observe");
        state.gather = animator.GetBool("Gather");


        ////No need to update again if last sent state == current view modified state
        if (localUpdatedState != null)
        {
            //TODO: Uses reflection so might be slow, replace with defined compare to improve speed
            List<PropertyCompareResult> changesLocal = Compare(localUpdatedState, state);
            if (changesLocal.Count == 0 || (changesLocal.Count == 1 && changesLocal[0].Name == "timestamp"))
            {
                return;
            }
        }


        //TODO: Uses reflection so might be slow, replace with defined compare to improve speed
        List<PropertyCompareResult> changes = Compare(previousState, state);

        //Transform has been update locally, push changes
        if (changes.Count > 0)
        {
            //Create Change Set Array for NetSend
            object[] changeSet = new object[(changes.Count * 2) + 1];
            changeSet[0] = state.id;
            int saveIndex = 1;
            for (int i = 0; i < changes.Count; i++)
            {
                changeSet[saveIndex] = changes[i].Name;
                changeSet[saveIndex + 1] = changes[i].NewValue;
                saveIndex += 2;
            }
            localUpdatedState = state.Clone();
            ColyseusManager.NetSend("entityUpdate", changeSet);
        }

    }

    protected virtual void Update()
    {

        if (IsMine)
        {
            counterStateSyncUpdateRate += Time.deltaTime;
            if (counterStateSyncUpdateRate > stateSyncUpdateRateMs / 1000f)
            {
                counterStateSyncUpdateRate = 0f;
                //Debug.Log($"Updating state from this view - {UserName}");
                UpdateStateFromView();
            }

        }
        else if (!IsMine && (syncLocalPosition))
        {
            // You are not the owner, so you have to converge the object's state toward the server's state.
            ProcessViewSync();
        }


        // Update the local position delta
        localPositionDelta = myTransform.localPosition - prevLocalPosition;
        prevLocalPosition = myTransform.localPosition;

    }

    public void RemoteFunctionCallHandler(RFCMessage _rfc)
    {
        System.Type thisType = this.GetType();
        //LSLog.Log("got RFC call for " + _rfc.function);

        MethodInfo theMethod = thisType.GetMethod(_rfc.function);
        if (theMethod != null)
            theMethod.Invoke(this, _rfc.param);
        else
            LSLog.LogError("Missing Fucntion: " + _rfc.function);
    }


    protected virtual void ProcessViewSync()
    {
        // This is the target playback time of this body
        double interpolationTime = ColyseusManager.Instance.GetRoundtripTime - interpolationBackTimeMs;

        // Use interpolation if the target playback time is present in the buffer
        if (proxyStates[0].timestamp > interpolationTime)
        {
            // The longer the time since last update add a delta factor to the lerp speed to get there quicker
            float deltaFactor = (ColyseusManager.Instance.GetServerTimeSeconds > proxyStates[0].timestamp) ?
                (float)(ColyseusManager.Instance.GetServerTimeSeconds - proxyStates[0].timestamp) * 0.2f : 0f;

            if (GameController.Instance.gamePlaying && syncLocalPosition) myTransform.localPosition = Vector3.Lerp(myTransform.localPosition, proxyStates[0].pos, Time.deltaTime * (positionLerpSpeed + deltaFactor));       
            //if (syncLocalRotation && Mathf.Abs(Quaternion.Angle(transform.localRotation, proxyStates[0].rot)) > snapIfAngleIsGreater) myTransform.localRotation = proxyStates[0].rot;

            //if (syncLocalRotation) myTransform.localRotation = Quaternion.Slerp(myTransform.localRotation, proxyStates[0].rot, Time.deltaTime * (rotationLerpSpeed + deltaFactor));
        }
        // Use extrapolation (If we didnt get a packet in the last X ms and object had velcoity)
        else
        {
            EntityState latest = proxyStates[0];

            float extrapolationLength = (float)(interpolationTime - latest.timestamp);
            // Don't extrapolation for more than 500 ms, you would need to do that carefully
            if (extrapolationLength < extrapolationLimitMs / 1000f)
            {
                if (syncLocalPosition) myTransform.localPosition = latest.pos + latest.vel * extrapolationLength;
                //if (syncLocalRotation) myTransform.localRotation = latest.rot;
            }
        }
    }

    protected T ParseRFCObject<T>(object raw)
        where T : class, new()
    {
        IDictionary<string, object> parseFrom = new Dictionary<string, object>();
        foreach (DictionaryEntry item in (IEnumerable)raw)
        {
            parseFrom.Add(item.Key.ToString(), item.Value);
        }

        return parseFrom.ToObject<T>();
    }


    public void SetAttributes(Dictionary<string, string> attributesToSet)
    {
        ColyseusManager.NetSend("setAttribute", new AttributeUpdateMessage() { entityId = state.id, attributesToSet = attributesToSet });
    }

    protected static List<PropertyCompareResult> Compare<T>(T oldObject, T newObject)
    {
        FieldInfo[] properties = typeof(T).GetFields();
        List<PropertyCompareResult> result = new List<PropertyCompareResult>();

        foreach (FieldInfo pi in properties)
        {

            object oldValue = pi.GetValue(oldObject), newValue = pi.GetValue(newObject);

            if (!object.Equals(oldValue, newValue))
            {
                result.Add(new PropertyCompareResult(pi.Name, oldValue, newValue));
            }
        }

        return result;
    }
}