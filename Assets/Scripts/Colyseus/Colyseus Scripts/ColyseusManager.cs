using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using GameDevWare.Serialization;
using LucidSightTools;
using NativeWebSocket;
using UnityEngine;

public class ColyseusManager : ColyseusManager<ColyseusManager>
{
        /// Network Events
    //==========================
    ///Delegate and event for Current User State Changing.
    public delegate void OnUserStateChanged(MapSchema<string> changes);
    public static event OnUserStateChanged onCurrentUserStateChanged;

    ///Delegate and event for Room State Updating
    public delegate void OnRoomChanged(ColyseusRoom<RoomState> room);
    public static event OnRoomChanged onRoomChanged;

    ///Delegate and event for Joining 
    public delegate void OnJoined(string customLogic);
    public static event OnJoined onJoined;

    ///Delegate and event for Player joining;
    public delegate void OnPlayerJoined(string playerUserName);
    public static event OnPlayerJoined onPlayerJoined;
    public delegate void OnRoomsReceived(ColyseusRoomAvailable[] rooms);
    public static event OnRoomsReceived onRoomsReceived;
    private NetworkedEntityFactory _networkedEntityFactory;
    private ColyseusClient _client;

    /// Returns a reference to the current networked user.
    public string _roomName = "NO_ROOM_NAME_PROVIDED";
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
    ///     Collection for tracking users that have joined the room.
    private IndexedDictionary<string, NetworkedUser> _users =
        new IndexedDictionary<string, NetworkedUser>();
    private Dictionary<string, object> roomOptionsDictionary = new Dictionary<string, object>();
    public RoomState currentRoomState;
    public List<IColyseusRoom> rooms = new List<IColyseusRoom>();
    private bool isInitialized;

    /// Returns true if there is an active room.
    public bool isInRoom
    {
        get{ return Room != null; }
    }

    /// Returns the synchronized time from the server in milliseconds.
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
    private double _serverTime = -1;
    public string LastRoomID
    {
        get { return _lastRoomId; }
    }
    /// Retunrs a reference to the current networked user.
    public NetworkedUser CurrentNetworkedUser
    {
        get { return _currentNetworkedUser; }
    }
    private NetworkedUser _currentNetworkedUser;

    public static bool IsReady
    {
        get{ return Instance != null; }
    }

    private string userName;

    /// Display name for the user - Check with Lobby Controller
    public string UserName
    {
        get{ return userName; }
        set{ userName = value; }
    }

    ///     Used to help calculate the latency of the connection to the server.
    private double _lastPing;

    ///     Used to help calculate the latency of the connection to the server.
    private double _lastPong;

    ///     The ID of the room we were just connected to.
    ///     If there is an abnormal disconnect from the current room
    ///     an automatic attempt will be made to reconnect to that room
    ///     with this room ID.
    private string _lastRoomId;

    ///     Thread responsible for running RunPingThread
    ///     on a ColyseusRoom{T}
    private Thread _pingThread;
    private bool _waitForPong;


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
    }

    public IEnumerator WaitThenSpawnPlayer(string entityID)
    {
        while (!Room.State.networkedEntities.ContainsKey(entityID))
        {
            //Wait until the room has a state for this ID (may take a frame or two, prevent race conditions)
            yield return new WaitForEndOfFrame();
        }
        Debug.Log(entityID + " and " + Room.SessionId);
        bool isOurs = entityID.Equals(Room.SessionId);
        Debug.Log(Room.State.networkedEntities[entityID]);
        NetworkedEntityState entityState = Room.State.networkedEntities[entityID];

        if (isOurs == false)
        {
            NetworkedEntityFactory.Instance.SpawnEntity(entityState, isOurs);
        }
        else
        {// Update our existing entity

            NetworkedEntityFactory.Instance.SpawnEntity(entityState, true);
        }
    }
/*
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
*/

    public void Initialize(string roomName, Dictionary<string, object> roomOptions)
    {
        if(isInitialized)
        {
            return;
        }
        _roomName = roomName;
        isInitialized = true;
        /// Set up room controller
        SetRoomOptions(roomOptions);
        SetDependencies(_colyseusSettings);
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

        SetClient(client);
    }

    /// Frame-rate independent message for physics
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        IncrementServerTime();
    }

    public async void GetAvailableRooms()
    {
        ColyseusRoomAvailable[] rooms = await client.GetAvailableRooms(_roomName);

        onRoomsReceived?.Invoke(rooms);
    }
/*
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
*/
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
        await JoinRoomId(roomID);
    }

    public async void CreateNewRoom(string roomID)
    {
        await CreateSpecificRoom(client, _roomName, roomID);
    }

    public async void JoinOrCreateRoom()
    {
        await JoinOrCreate();
    }

    public async void LeaveAllRooms(Action onLeave)
    {
        await LeaveAllRooms(true, onLeave);
    }

    /// On detection of OnApplicationQuit will disconnect from all rooms
    private void CleanUpOnAppQuit()
    {
        if (client == null)
        {
            return;
        }

        CleanUp();
    }

    /// Monobehaviour callback that gets called just before app exit
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        LeaveAllRooms(true);

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
        if (Instance.Room == null)
        {
            LSLog.LogError($"Error: Not in room for action {action} msg {message}");
            return;
        }

        _ = message == null
            ? Instance.Room.Send(action)
            : Instance.Room.Send(action, message);
    }

    /// Send an action and message object to the room.
    /// actionByte - The action to take
    /// The message object to pass along to the room
    public static void NetSend(byte actionByte, object message = null)
    {
        if (Instance.Room == null)
        {
            LSLog.LogError(
                $"Error: Not in room for action bytes msg {(message != null ? message.ToString() : "No Message")}");
            return;
        }

        _ = message == null
            ? Instance.Room.Send(actionByte)
            : Instance.Room.Send(actionByte, message);
    }

    public static event OnUserStateChanged OnCurrentUserStateChanged;

    ///     Set the dependencies.
    ///     roomName
    ///     settings
    public void SetDependencies(ColyseusSettings settings)
    {
        _colyseusSettings = settings;

        ColyseusClient.onAddRoom += AddRoom;
    }

    public void SetRoomOptions(Dictionary<string, object> options)
    {
        roomOptionsDictionary = options;
    }

    ///     Set the client of the ColyseusRoomController
    public void SetClient(ColyseusClient client)
    {
        _client = client;
    }

    ///     Adds the given room to rooms and
    ///     initiates its connection to the server
    public void AddRoom(IColyseusRoom roomToAdd)
    {
        roomToAdd.OnLeave += code => rooms.Remove(roomToAdd);
        rooms.Add(roomToAdd);
    }

    ///     Create a room with the given roomId.
    ///     The ID for the room.
    public async Task CreateSpecificRoom(ColyseusClient client, string roomName, string roomId,
        Action<bool> onComplete = null)
    {
        LSLog.LogImportant($"Creating Room {roomId}");

        try
        {
            //Populate an options dictionary with custom options provided elsewhere as well as the critical option we need here, roomId
            Dictionary<string, object> options = new Dictionary<string, object> {["roomId"] = roomId};
            foreach (KeyValuePair<string, object> option in roomOptionsDictionary)
            {
                options.Add(option.Key, option.Value);
            }

            _room = await client.Create<RoomState>(roomName, options);
        }
        catch (Exception ex)
        {
            LSLog.LogError($"Failed to create room {roomId} : {ex.Message}");
            onComplete?.Invoke(false);
            return;
        }

        onComplete?.Invoke(true);
        LSLog.LogImportant($"Created Room: {_room.Id}");
        _lastRoomId = roomId;
        currentRoomState = Room.State;
        RegisterRoomHandlers();
    }

    ///     Join an existing room or create a new one using roomName with no options.
    ///     Locked or private rooms are ignored.
    public async Task JoinOrCreate(Action<bool> onComplete = null)
    {
        LSLog.LogImportant($"Join Or Create Room - Name = {_roomName}.... ");
        try
        {
            // Populate an options dictionary with custom options provided elsewhere
            Dictionary<string, object> options = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> option in roomOptionsDictionary)
            {
                options.Add(option.Key, option.Value);
            }

            _room = await _client.JoinOrCreate<RoomState>(_roomName, options);
        }
        catch (Exception ex)
        {
            LSLog.LogError($"Room Controller Error - {ex.Message + ex.StackTrace}");
            onComplete?.Invoke(false);
            return;
        }

        onComplete?.Invoke(true);
        LSLog.LogImportant($"Joined / Created Room: {_room.Id}");
        _lastRoomId = _room.Id;
        currentRoomState = Room.State;
        RegisterRoomHandlers();
    }

        public async Task JoinRoomId(string roomId, Action<bool> onJoin = null)
    {
        LSLog.Log($"Joining Room ID {roomId}....");
        ClearRoomHandlers();

        try
        {
            while (_room == null || !_room.colyseusConnection.IsOpen)
            {
                _room = await _client.JoinById<RoomState>(roomId, null);

                if (_room == null || !_room.colyseusConnection.IsOpen)
                {
                    LSLog.LogImportant($"Failed to Connect to {roomId}.. Retrying in 5 Seconds...");
                    await Task.Delay(5000);
                }
            }

            _lastRoomId = roomId;
            currentRoomState = Room.State;
            RegisterRoomHandlers();
            onJoin?.Invoke(true);
        }
        catch (Exception ex)
        {
            LSLog.LogError(ex.Message);
            onJoin?.Invoke(false);
            //LSLog.LogError("Failed to joining room, try another...");
            //await CreateSpecificRoom(_client, roomName, roomId, onJoin);
        }
    }

    public async Task LeaveAllRooms(bool consented, Action onLeave = null)
    {
        if (_room != null && rooms.Contains(_room) == false)
        {
            await _room.Leave(consented);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            await rooms[i].Leave(consented);
        }

        ClearRoomHandlers();
        onLeave?.Invoke();
    }

    ///     Subscribes the manager to room networked events
    ///     and starts measuring latency to the server.
    public virtual void RegisterRoomHandlers()
    {
        LSLog.LogImportant($"sessionId: {_room.SessionId}");

        if (_pingThread != null)
        {
            _pingThread.Abort();
            _pingThread = null;
        }

        _pingThread = new Thread(RunPingThread);
        _pingThread.Start(_room);

        _room.OnLeave += OnLeaveRoom;

        _room.OnStateChange += OnStateChangeHandler;

        _room.OnMessage<NetworkedUser>("onJoin", currentNetworkedUser =>
        {
            Debug.Log($"Received 'NetworkedUser' after join/creation call {currentNetworkedUser.sessionId}!");
            Debug.Log(Json.SerializeToString(currentNetworkedUser));

            _currentNetworkedUser = currentNetworkedUser;
        });
/*
        _room.OnMessage<RFCMessage>("onRFC", _rfc =>
        {
            //Debug.Log($"Received 'onRFC' {_rfc.entityId}!");
            if (_entityViews.Keys.Contains(_rfc.entityId))
            {
                ///_entityViews[_rfc.entityId].RemoteFunctionCallHandler(_rfc);
            }
        });
*/
        _room.OnMessage<PongMessage>(0, message =>
        {
            _lastPong = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _serverTime = message.serverTime;
            _waitForPong = false;
        });

        //Custom game logic
        //_room.OnMessage<YOUR_CUSTOM_MESSAGE>("messageNameInCustomLogic", objectOfTypeYOUR_CUSTOM_MESSAGE => {  });

        //========================
        _room.State.networkedEntities.OnAdd += OnEntityAdd;
        _room.State.networkedEntities.OnRemove += OnEntityRemoved;

        _room.State.networkedUsers.OnAdd += OnUserAdd;
        _room.State.networkedUsers.OnRemove += OnUserRemove;

        _room.State.TriggerAll();
        //========================

        _room.colyseusConnection.OnError += Room_OnError;
        _room.colyseusConnection.OnClose += Room_OnClose;
    }

    private void OnLeaveRoom(int code)
    {
        WebSocketCloseCode parsedCode = WebSocketHelpers.ParseCloseCodeEnum(code);
        LSLog.Log(string.Format("ROOM: ON LEAVE =- Reason: {0} ({1})", parsedCode, code));
        _pingThread.Abort();
        _pingThread = null;
        _room = null;

        if (parsedCode != WebSocketCloseCode.Normal && !string.IsNullOrEmpty(_lastRoomId))
        {
            JoinRoomId(_lastRoomId);
        }
    }

    ///     Unsubscribes Room from networked events."
    private void ClearRoomHandlers()
    {
        if (_pingThread != null)
        {
            _pingThread.Abort();
            _pingThread = null;
        }

        if (_room == null)
        {
            return;
        }

        ///_room.State.networkedEntities.OnAdd -= OnEntityAdd;
        ///_room.State.networkedEntities.OnRemove -= OnEntityRemoved;
        _room.State.networkedUsers.OnAdd -= OnUserAdd;
        _room.State.networkedUsers.OnRemove -= OnUserRemove;

        _room.colyseusConnection.OnError -= Room_OnError;
        _room.colyseusConnection.OnClose -= Room_OnClose;

        _room.OnStateChange -= OnStateChangeHandler;

        _room.OnLeave -= OnLeaveRoom;

        _room = null;
        _currentNetworkedUser = null;
    }


    ///     Asynchronously gets all the available rooms of the _client
    ///     named roomName
    public async Task<ColyseusRoomAvailable[]> GetRoomListAsync()
    {
        ColyseusRoomAvailable[] allRooms = await _client.GetAvailableRooms(_roomName);

        return allRooms;
    }


    private void OnUserAdd(string key, NetworkedUser user)
    {
        LSLog.LogImportant($"user [{user.__refId} | {user.sessionId} | key {key}] Joined");

        // Add "player" to map of players
        _users.Add(key, user);
        Debug.Log($"User added now spawning - key is {key} and user id is {user.sessionId}");
        StartCoroutine(WaitThenSpawnPlayer(user.sessionId));

        // On entity update...
        user.OnChange += changes =>
        {
            user.updateHash = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // If the change is for our current user then fire the event with the attributes that changed
            if (Instance.CurrentNetworkedUser != null &&
                string.Equals(Instance.CurrentNetworkedUser.sessionId, user.sessionId))
            {
                OnCurrentUserStateChanged?.Invoke(user.attributes);
            }
        };
    }

    private void OnUserRemove(string key, NetworkedUser user)
    {
        LSLog.LogImportant($"user [{user.__refId} | {user.sessionId} | key {key}] Left");

        _users.Remove(key);
    }

    ///     Callback for when the room's connection closes.
    ///     closeCode - Code reason for the connection close.
    private static void Room_OnClose(int closeCode)
    {
        LSLog.LogError("Room_OnClose: " + closeCode);
    }

    ///     Callback for when the room get an error.
    ///     errorMsg - The error message.
    private static void Room_OnError(string errorMsg)
    {
        LSLog.LogError("Room_OnError: " + errorMsg);
    }

    ///     Callback when the room state has changed.
    ///     state - The room state.
    ///     isFirstState - Is it the first state?
    private static void OnStateChangeHandler(RoomState state, bool isFirstState)
    {

        LSLog.LogImportant("State has been updated!");
    }

    ///     Sends "ping" message to current room to help measure latency to the server.
    ///     roomToPing - The ColyseusRoom{T} to ping.
    private void RunPingThread(object roomToPing)
    {
        ColyseusRoom<RoomState> currentRoom = (ColyseusRoom<RoomState>) roomToPing;

        const float pingInterval = 0.5f; // seconds
        const float pingTimeout = 15f; //seconds

        int timeoutMilliseconds = Mathf.FloorToInt(pingTimeout * 1000);
        int intervalMilliseconds = Mathf.FloorToInt(pingInterval * 1000);

        DateTime pingStart;
        while (currentRoom != null)
        {
            _waitForPong = true;
            pingStart = DateTime.Now;
            _lastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _ = currentRoom.Send("ping");

            while (currentRoom != null && _waitForPong &&
                   DateTime.Now.Subtract(pingStart).TotalSeconds < timeoutMilliseconds)
            {
                Thread.Sleep(200);
            }

            if (_waitForPong)
            {
                LSLog.LogError("Ping Timed out");
            }

            Thread.Sleep(intervalMilliseconds);
        }
    }

    ///     Increments the known _serverTime by Time.fixedDeltaTime
    ///     converted into milliseconds.
    public void IncrementServerTime()
    {
        _serverTime += Time.fixedDeltaTime * 1000;
    }

    public async void CleanUp()
    {
        _pingThread?.Abort();

        List<Task> leaveRoomTasks = new List<Task>();

        foreach (IColyseusRoom roomEl in rooms)
        {
            leaveRoomTasks.Add(roomEl.Leave(false));
        }

        if (_room != null)
        {
            leaveRoomTasks.Add(_room.Leave(false));
        }

        await Task.WhenAll(leaveRoomTasks.ToArray());
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