using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus;
using LucidSightTools;
using UnityEngine;

public class ColyseusManager : ColyseusManager<ColyseusManager>
{
    public delegate void OnRoomChanged(ColyseusRoom<RoomState> room);
    public static event OnRoomChanged onRoomChanged;
    public delegate void OnRoomsReceived(ColyseusRoomAvailable[] rooms);
    public static event OnRoomsReceived onRoomsReceived;
    private NetworkedEntityFactory _networkedEntityFactory;

    [SerializeField] 
    private RoomController _roomController;

    /// Returns a reference to the current networked user.
    public NetworkedEntity CurrentNetworkedEntity;

    public ColyseusRoom<RoomState> Room
    {
        get
        {
            return _room;
        }

        private set
        {
            _room = value;
        }
    }

    private ColyseusRoom<RoomState> _room;

    private bool isInitialized;

    /// Returns true if there is an active room.
    public bool isInRoom
    {
        get{ return _roomController.Room != null; }
    }

    /// Returns the synchronized time from the server in milliseconds.
    public double GetServerTime
    {
        get{ return _roomController.GetRoundtripTime; }
    }

    ///     Returns the synchronized time from the server in seconds.
    public double GetServerTimeSeconds
    {
        get { return _roomController.GetServerTimeSeconds; }
    }

    /// Retunrs a reference to the current networked user.
    public NetworkedEntityState CurrentUser
    {
        get{ return _roomController.CurrentNetworkedUser; }
    }

    public static bool IsReady
    {
        get{ return Instance != null; }
    }

    private string userName;

    /// Display name for the user
    public string UserName
    {
        get{ return userName; }
        set{ userName = value; }
    }
    public RoomState currentRoomState;

    public float ServerTime
    {
        get
        {
            if (currentRoomState != null)
            {
                return currentRoomState.serverTime;
            }
            
            LSLog.Log("Asked for server time but no room yet!", LSLog.LogColor.yellow);
            return 0;
        }
    }

    protected override void Awake() {
        base.Awake();


    }

    protected override void Start()
    {
        base.Start();
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnregisterHandlers();
    }

    private IEnumerator WaitThenSpawnPlayer(string entityID)
    {
        while (!Room.State.networkedUsers.ContainsKey(entityID))
        {
            //Wait until the room has a state for this ID (may take a frame or two, prevent race conditions)
            yield return new WaitForEndOfFrame();
        }

        bool isOurs = entityID.Equals(Room.SessionId);
        NetworkedEntityState entityState = Room.State.networkedUsers[entityID];

        if (isOurs == false)
        {
            Debug.Log("Spawning");
            NetworkedEntityFactory.Instance.SpawnEntity(entityState, isOurs);
        }
        else
        {// Update our existing entity

            if (NetworkedEntityFactory.Instance.UpdateOurEntity(entityState) == false)
            {// Spawn a new entity for us since something went wrong attempting to update our existing one
                NetworkedEntityFactory.Instance.SpawnEntity(entityState, true);
            }
        }
    }

    public async void ConsumeSeatReservation(ColyseusRoomAvailable room, string sessionId)
    {
        try
        {
            ColyseusMatchMakeResponse response = new ColyseusMatchMakeResponse() { room = room, sessionId = sessionId };

            Room = await client.ConsumeSeatReservation<RoomState>(response);

            onRoomChanged?.Invoke(Room);

            currentRoomState = Room.State;
            RegisterHandlers();
        }
        catch (System.Exception error)
        {
            LSLog.LogError($"Error attempting to consume seat reservation - {error.Message + error.StackTrace}");
        }
    }

    public void Initialize(string roomName, Dictionary<string, object> roomOptions)
    {
        if(isInitialized)
        {
            return;
        }

        isInitialized = true;
        /// Set up room controller
        _roomController = new RoomController {roomName = roomName};
        _roomController.SetRoomOptions(roomOptions);
        _roomController.SetDependencies(_colyseusSettings);
        RegisterHandlers();

/*
        /// Set up Networked Entity Factory
        _networkedEntityFactory = new NetworkedEntityFactory(_roomController.CreationCallbacks, 
            _roomController.Entities, _roomController.EntityViews);
 */
    }

    /// Create a new Colyseus Client
    public override void InitializeClient()
    {
        base.InitializeClient();

        _roomController.SetClient(client);
    }

    /// Frame-rate independent message for physics
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        _roomController.IncrementServerTime();
    }

    public async void GetAvailableRooms()
    {
        ColyseusRoomAvailable[] rooms = await client.GetAvailableRooms(_roomController.roomName);

        onRoomsReceived?.Invoke(rooms);
    }

    private void RegisterHandlers()
    {
        if (Room != null)
        {
            Room.OnStateChange += OnRoomStateChange;
            Room.State.networkedUsers.OnAdd += NetworkedUsers_OnAdd;
            Room.State.networkedUsers.OnRemove += NetworkedUsers_OnRemove;
/*
            _roomController.Room.OnMessage<ObjectUseMessage>("objectUsed", (msg) =>
            {
                StartCoroutine(AwaitObjectInteraction(msg.interactedObjectID, msg.interactingStateID));
            });
            _roomController.Room.OnMessage<MovedToGridMessage>("movedToGrid", OnMovedToGrid);
*/
        }
        else
        {
            LSLog.LogError($"Cannot register room handlers, room is null!");
        }
    }

        private void UnregisterHandlers()
    {
        if (Room != null)
        {
            Room.OnStateChange -= OnRoomStateChange;

            Room.State.networkedUsers.OnAdd -= NetworkedUsers_OnAdd;
            Room.State.networkedUsers.OnRemove -= NetworkedUsers_OnRemove;

        }
    }

       /// <summary>
    /// Callback for when a networked entity has been removed from the room state's collection of networked entities/users
    /// </summary>
    /// <param name="key">The sessionId of the networked entity that got removed</param>
    /// <param name="value">The <see cref="NetworkedEntityState"/> of the user that was removed</param>
    private void NetworkedUsers_OnRemove(string key, NetworkedEntityState value)
    {
        NetworkedEntityFactory.Instance.RemoveEntity(value.id);
    }

    /// <summary>
    /// Callback for when a networked entity has been added to the room state's collection of networked entities/users
    /// </summary>
    /// <param name="key">The sessionId of the networked entity that got added</param>
    /// <param name="value">The <see cref="NetworkedEntityState"/> of the user that was added</param>
    private void NetworkedUsers_OnAdd(string key, NetworkedEntityState value)
    {
        // TODO: subscribe to NetworkedEntityState OnChange event for updating entities
        Debug.Log("Starting spawn coroutine");
        StartCoroutine(WaitThenSpawnPlayer(value.id));

        //if (value.id.Equals(_room.SessionId))
        //{
        //    JoinChatRoom();
        //}
    }

    /// <summary>
    /// Event handler when the room receives its first state
    /// </summary>
    /// <param name="state"></param>
    /// <param name="isfirststate"></param>
    private void OnRoomStateChange(RoomState state, bool isfirststate)
    {
        if (isfirststate)
        {
            //LSLog.LogImportant($"On Room State Changed - First State!", LSLog.LogColor.yellow);
        }
    }


    public async void JoinExistingRoom(string roomID)
    {
        await _roomController.JoinRoomId(roomID);
    }

    public async void CreateNewRoom(string roomID)
    {
        await _roomController.CreateSpecificRoom(client, _roomController.roomName, roomID);
    }

    public async void JoinOrCreateRoom()
    {
        await _roomController.JoinOrCreateRoom();
    }

    public async void LeaveAllRooms(Action onLeave)
    {
        await _roomController.LeaveAllRooms(true, onLeave);
    }

    /// Checks if ColyseusNetworkedEntityView exists for the given ID
    public bool HasEntityView(string entityId)
    {
        return _roomController.HasEntityView(entityId);
    }

    /// Returns a NetworkedEntityView given entityId
    /// Returns NetworkedEntityView if one exists for the given id
    public NetworkedEntityView GetEntityView(string entityId)
    {
        return _roomController.GetEntityView(entityId);
    }

    /// On detection of OnApplicationQuit will disconnect from all rooms
    private void CleanUpOnAppQuit()
    {
        if (client == null)
        {
            return;
        }

        _roomController.CleanUp();
    }

    /// Monobehaviour callback that gets called just before app exit
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        _roomController.LeaveAllRooms(true);

        CleanUpOnAppQuit();
    }

#if UNITY_EDITOR
    public void OnEditorQuit()
    {
        OnApplicationQuit();
    }
#endif

#region Remote Function Call
    /// Send a remote function call
    /// Entity - the entity we want to send the RFC
    /// function - the name of the function to call
    /// param - the parameters of the function to call
    /// target - who should receive this RFC
    public static void RFC(StoneColyseusNetworkedEntityView entity, string function, object[] param,
        RFCTargets target = RFCTargets.ALL)
    {
        RFC(entity.Id, function, param, target);
    }

    /// Send a Remote Function Call
    /// The ID of the entity we want to send the RFC
    /// The name of the function to call
    /// The parameters of the function to call
    /// Who should receive this RFC
    public static void RFC(string entityId, string function, object[] param,
        RFCTargets target = RFCTargets.ALL)
    {
        NetSend("remoteFunctionCall",
            new RFCMessage {entityId = entityId, function = function, param = param, target = target});
    }

    public static void CustomServerMethod(string methodName, object[] param)
    {
        NetSend("customMethod", new CustomMethodMessage {method = methodName, param = param});
    }

    /// Send an action and message object to the room.
    /// The action to take
    /// The message object to pass along to the room
    public static void NetSend(string action, object message = null)
    {
        if (Instance._roomController.Room == null)
        {
            LSLog.LogError($"Error: Not in room for action {action} msg {message}");
            return;
        }

        _ = message == null
            ? Instance._roomController.Room.Send(action)
            : Instance._roomController.Room.Send(action, message);
    }

    /// Send an action and message object to the room.
    /// actionByte - The action to take
    /// The message object to pass along to the room
    public static void NetSend(byte actionByte, object message = null)
    {
        if (Instance._roomController.Room == null)
        {
            LSLog.LogError(
                $"Error: Not in room for action bytes msg {(message != null ? message.ToString() : "No Message")}");
            return;
        }

        _ = message == null
            ? Instance._roomController.Room.Send(actionByte)
            : Instance._roomController.Room.Send(actionByte, message);
    }

#endregion Remote Function Call
/*
#region Networked Entity Creation
    /// Creates a new NetworkedEntity with the given prefab and attributes.
    /// Prefab you would like to use to create the entity
    /// Entity attributes
    public void InstantiateNetworkedEntity(string prefab, Dictionary<string, object> attributes = null)
    {
        InstantiateNetworkedEntity(prefab, Vector2.zero, attributes);
    }

    /// Creates a new NetworkedEntity with the given prefab and attributes
    /// and places it at the provided position.
    /// prefab - Prefab you would like to use to create the entity
    /// position - Position for the new entity
    /// attributes - Entity attributes
    public static void InstantiateNetworkedEntity(string prefab, Vector2 position,
        Dictionary<string, object> attributes = null)
    {
        Instance._networkedEntityFactory.InstantiateNetworkedEntity(Instance._roomController.Room, prefab, position, attributes);
    }

    /// Creates a new NetworkedEntity with the given ColyseusNetworkedEntityView and attributes 
    /// and places it at the provided position and rotation.
    /// viewToAssign - The provided view that will be assigned to the new NetworkedEntity
    /// callback - Callback that will be invoked with the newly created NetworkedEntity
    public static void CreateNetworkedEntityWithTransform(Vector2 position,
        Dictionary<string, object> attributes = null, StoneColyseusNetworkedEntityView viewToAssign = null,
        Action<NetworkedEntity> callback = null)
    {
        Instance._networkedEntityFactory.CreateNetworkedEntityWithTransform(Instance._roomController.Room, position,
            attributes, viewToAssign, callback);
    }

    /// Creates a new NetworkedEntity with the given prefab, attributes, and ColyseusNetworkedEntityView
    /// prefab - Prefab you would like to use
    /// attributes - Position for the new entity
    /// viewToAssign - The provided view that will be assigned to the new NetworkedEntity
    /// callback - Callback that will be invoked with the newly created NetworkedEntity
    public static void CreateNetworkedEntity(string prefab, Dictionary<string, object> attributes = null,
        StoneColyseusNetworkedEntityView viewToAssign = null, Action<NetworkedEntity> callback = null)
    {
        Instance._networkedEntityFactory.CreateNetworkedEntity(Instance._roomController.Room, prefab, attributes,
            viewToAssign, callback);
    }

    /// Creates a new NetworkedEntity attributes and ColyseusNetworkedEntityView
    /// attributes - Position for the new entity
    /// viewToAssign - The provided view that will be assigned to the new NetworkedEntity
    /// callback - Callback that will be invoked with the newly created NetworkedEntity
    public static void CreateNetworkedEntity(Dictionary<string, object> attributes = null,
        StoneColyseusNetworkedEntityView viewToAssign = null, Action<NetworkedEntity> callback = null)
    {
        Instance._networkedEntityFactory.CreateNetworkedEntity(Instance._roomController.Room, attributes, viewToAssign,
            callback);
    }

    /// Registers the ColyseusNetworkedEntityView with the manager for tracking.
    /// Initializes the ColyseusNetworkedEntityView if it has not yet been initialized.
    public void RegisterNetworkedEntityView(NetworkedEntity model, StoneColyseusNetworkedEntityView view)
    {
        _networkedEntityFactory.RegisterNetworkedEntityView(model, view);
    }

    /// Creates a GameObject using the ColyseusNetworkedEntityView's prefab.
    /// Requires that the entity has a "prefab" attribute defined.
    private static async void CreateFromPrefab(NetworkedEntity entity)
    {
        await Instance._networkedEntityFactory.CreateFromPrefab(entity);
    }

#endregion Networked Entity Creation
*/
}