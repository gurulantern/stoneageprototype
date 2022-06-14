using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using GameDevWare.Serialization;
using LucidSightTools;
using NativeWebSocket;
using UnityEngine;

///     Manages the rooms of a server connection.
[Serializable] public class RoomController
{
    /// Network Events
    //==========================
    /// OnNetworkEntityAdd delegate for OnNetworkEntityAdd event.
    /// Then entity that was just added to the room.
    public delegate void OnNetworkEntityAdd(NetworkedEntity entity);
    /// Event for when a NetworkEntity is added to the room.
    public static OnNetworkEntityAdd onAddNetworkEntity;

    /// OnNetworkEntityRemoved delegate for OnNetworkEntityRemoved event.
    /// Then entity that was just removed to the room.
    public delegate void OnNetworkEntityRemoved(NetworkedEntity entity, StoneColyseusNetworkedEntityView view);
    /// Event for when a NetworkEntity is removed from the room.
    public static OnNetworkEntityRemoved onRemoveNetworkEntity;

    ///Delegate and event for Current User State Changing.
    public delegate void OnUserStateChanged(MapSchema<string> changes);
    public static event OnUserStateChanged onCurrentUserStateChanged;
    ///Delegate and event for Joining 
    public delegate void OnJoined(string customLogic);
    public static event OnJoined onJoined;

    ///Delegate and event for Player joining;
    public delegate void OnPlayerJoined(string playerUserName);
    public static event OnPlayerJoined onPlayerJoined;

    ///Delegate and event for Updating team.
    public delegate void OnTeamUpdate(int teamIndex, string clientID, bool added);
    public static event OnTeamUpdate onTeamUpdate;

    ///Delegate and event for Team received.
    public delegate void OnTeamReceive(int teamIndex, string[] clients);
    public static event OnTeamReceive onTeamReceive;




    /// Our user object we get upon joining a room.
    [SerializeField] private static NetworkedUser _currentNetworkedUser;

    ///  The Client that is created when connecting to the Colyseus server.
    private ColyseusClient _client;
    private ColyseusSettings _colyseusSettings;

    ///     Collection of entity creation callbacks. Callbacks are added to
    ///     the collection when a NetworkedEntity is created.
    ///     The callbacks are invoked and removed from the collection once the
    ///     entity has been added to the room.
    private Dictionary<string, Action<NetworkedEntity>> _creationCallbacks =
        new Dictionary<string, Action<NetworkedEntity>>();
    //==========================

    // TODO: Replace GameDevWare stuff
    ///     Collection for tracking entities that have been added to the room.
    private IndexedDictionary<string, NetworkedEntity> _entities =
        new IndexedDictionary<string, NetworkedEntity>();

    ///     Collection for tracking entity views that have been added to the room.
    private IndexedDictionary<string, NetworkedEntityView> _entityViews =
        new IndexedDictionary<string, NetworkedEntityView>();

    private NetworkedEntityFactory _factory;

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

    ///     The current or active Room we get when joining or creating a room
    private ColyseusRoom<RoomState> _room;

    ///     The time as received from the server in milliseconds.
    private double _serverTime = -1;

    ///     Collection for tracking users that have joined the room.
    private IndexedDictionary<string, NetworkedUser> _users =
        new IndexedDictionary<string, NetworkedUser>();

    ///     Used to help calculate the latency of the connection to the server.
    private bool _waitForPong;

    ///     The name of the room clients will attempt to create or join on the Colyseus server.
    public string roomName = "NO_ROOM_NAME_PROVIDED";

    private Dictionary<string, object> roomOptionsDictionary = new Dictionary<string, object>();

    ///     All the connected rooms.
    public List<IColyseusRoom> rooms = new List<IColyseusRoom>();

    ///     Returns the synchronized time from the server in milliseconds.
    public double GetServerTime
    {
        get { return _serverTime; }
    }

    ///     Returns the synchronized time from the server in seconds.
    public double GetServerTimeSeconds
    {
        get { return _serverTime / 1000; }
    }

    ///     The latency in milliseconds between client and server.
    public double GetRoundtripTime
    {
        get { return _lastPong - _lastPing; }
    }

    public ColyseusRoom<RoomState> Room
    {
        get { return _room; }
    }

    public string LastRoomID
    {
        get { return _lastRoomId; }
    }

    public IndexedDictionary<string, NetworkedEntity> Entities
    {
        get { return _entities; }
    }

    public IndexedDictionary<string, NetworkedEntityView> EntityViews
    {
        get { return _entityViews; }
    }

    public Dictionary<string, Action<NetworkedEntity>> CreationCallbacks
    {
        get { return _creationCallbacks; }
    }

    public NetworkedUser CurrentNetworkedUser
    {
        get { return _currentNetworkedUser; }
    }

    ///     Checks if a NetworkedEntityView exists for
    ///     the given ID
    ///      The ID of the NetworkedEntity we're checking for.
    public bool HasEntityView(string entityId)
    {
        return EntityViews.ContainsKey(entityId);
    }

    ///     Returns a NetworkedEntityView given
    ///     Returns NetworkedEntityView if one exists for the given entityId
    public NetworkedEntityView GetEntityView(string entityId)
    {
        if (EntityViews.ContainsKey(entityId))
        {
            return EntityViews[entityId];
        }

        return null;
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

    ///     Set the NetworkedEntityFactoryy of the RoomManager.
    public void SetNetworkedEntityFactory(NetworkedEntityFactory factory)
    {
        _factory = factory;
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
        RegisterRoomHandlers();
    }

    ///     Join an existing room or create a new one using roomName with no options.
    ///     Locked or private rooms are ignored.
    public async Task JoinOrCreateRoom(Action<bool> onComplete = null)
    {
        LSLog.LogImportant($"Join Or Create Room - Name = {roomName}.... ");
        try
        {
            // Populate an options dictionary with custom options provided elsewhere
            Dictionary<string, object> options = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> option in roomOptionsDictionary)
            {
                options.Add(option.Key, option.Value);
            }

            _room = await _client.JoinOrCreate<RoomState>(roomName, options);
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
        RegisterRoomHandlers();
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

        _room.OnMessage<OnJoinMessage>("onJoin", msg =>
        {
            _currentNetworkedUser = msg.newNetworkedUser;
            Debug.Log("Player has joined");
            onJoined?.Invoke(msg.customLogic);

/*
            Debug.Log($"Received 'NetworkedUser' after join/creation call {currentNetworkedUser.sessionId}!");
            Debug.Log(Json.SerializeToString(currentNetworkedUser));

            _currentNetworkedUser = currentNetworkedUser;
*/  
        });

        _room.OnMessage<RFCMessage>("onRFC", _rfc =>
        {
            //Debug.Log($"Received 'onRFC' {_rfc.entityId}!");
            if (_entityViews.Keys.Contains(_rfc.entityId))
            {
                _entityViews[_rfc.entityId].RemoteFunctionCallHandler(_rfc);
            }
        });

        _room.OnMessage<PongMessage>(0, message =>
        {
            _lastPong = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _serverTime = message.serverTime;
            _waitForPong = false;
        });

        //Custom game logic
        //_room.OnMessage<YOUR_CUSTOM_MESSAGE>("messageNameInCustomLogic", objectOfTypeYOUR_CUSTOM_MESSAGE => {  });
        _room.OnMessage<StoneAgePlayerJoinedMessage>("playerJoined", msg => { onPlayerJoined?.Invoke(msg.userName); });

        _room.OnMessage<StoneAgeTeamUpdateMessage>("onTeamUpdate", msg =>
        {
            LSLog.Log($"Updating team: {msg.teamIndex}, Client: {msg.clientID}, Added ? {msg.added}");
            onTeamUpdate?.Invoke(msg.teamIndex, msg.clientID, msg.added);
        });

        _room.OnMessage<StoneAgeAllTeamsUpdateMessage>("onReceiveTeam", msg =>
        {
            LSLog.Log($"Receiving full team: {msg.teamIndex}, Clients on Team: {msg.clients.Length}");
            onTeamReceive?.Invoke(msg.teamIndex, msg.clients);
        });

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

        _room.State.networkedEntities.OnAdd -= OnEntityAdd;
        _room.State.networkedEntities.OnRemove -= OnEntityRemoved;
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
        ColyseusRoomAvailable[] allRooms = await _client.GetAvailableRooms(roomName);

        return allRooms;
    }

    ///     Join a room with the given roomId"
    ///     roomId">ID of the room to join.
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

    ///     The callback for the event when a NetworkedEntity is added to a room.
    ///     The entity that was just added.
    ///     The entity's key
    private async void OnEntityAdd(string key, NetworkedEntity entity)
    {
        LSLog.LogImportant(
            $"Entity [{entity.__refId} | {entity.id}] add: x => {entity.xPos}, y => {entity.yPos}, z => {entity.zPos}");

        _entities.Add(entity.id, entity);

        //Creation ID is only Registered with the owner so only owners callback will be triggered
        if (!string.IsNullOrEmpty(entity.creationId) && _creationCallbacks.ContainsKey(entity.creationId))
        {
            Debug.Log("Creation Callbacks!");
            _creationCallbacks[entity.creationId].Invoke(entity);
            _creationCallbacks.Remove(entity.creationId);
        }

        onAddNetworkEntity?.Invoke(entity);

        if (_entityViews.ContainsKey(entity.id) == false && !string.IsNullOrEmpty(entity.attributes["prefab"]))
        {
            await _factory.CreateFromPrefab(entity);
        }
    }

    ///     The callback for the event when a NetworkedEntity is removed from a room.
    ///     entity - The entity that was just removed
    ///     key - The entity's key
    private void OnEntityRemoved(string key, NetworkedEntity entity)
    {
        if (_entities.ContainsKey(entity.id))
        {
            _entities.Remove(entity.id);
        }

        StoneColyseusNetworkedEntityView view = null;

        if (_entityViews.ContainsKey(entity.id))
        {
            view = _entityViews[entity.id];
            _entityViews.Remove(entity.id);
        }

        onRemoveNetworkEntity?.Invoke(entity, view);
    }

    ///     Callback for when a NetworkedUser is added to a room.
    ///     user - The user object
    ///     key - The user key
    private void OnUserAdd(string key, NetworkedUser user)
    {
        LSLog.LogImportant($"user [{user.__refId} | {user.sessionId} | key {key}] Joined");

        // Add "player" to map of players
        _users.Add(key, user);

        // On entity update...
        user.OnChange += changes =>
        {
            user.updateHash = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // If the change is for our current user then fire the event with the attributes that changed
            if (ColyseusManager.Instance.CurrentUser != null &&
                string.Equals(ColyseusManager.Instance.CurrentUser.sessionId, user.sessionId))
            {
                OnCurrentUserStateChanged?.Invoke(user.attributes);
            }
        };
    }

    ///     Callback for when a user is removed from a room.
    ///     user - The removed user.
    ///     key - The user key.
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
}