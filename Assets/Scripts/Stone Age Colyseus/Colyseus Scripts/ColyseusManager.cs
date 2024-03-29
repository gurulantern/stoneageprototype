using System;
using System.Collections.Generic;
using Colyseus;
using LucidSightTools;
using UnityEngine;

public class ColyseusManager : ColyseusManager<ColyseusManager>
{
    public delegate void OnRoomsReceived(StoneAgeRoomAvailable[] rooms);
    public static OnRoomsReceived onRoomsReceived;
    private NetworkedEntityFactory _networkedEntityFactory;

    [SerializeField] private RoomController _roomController;

    /// Returns a reference to the current networked user.
    public NetworkedEntity CurrentNetworkedEntity;

    private bool isInitialized;

    /// Returns true if there is an active room.
    public bool isInRoom
    {
        get{ return _roomController.Room != null; }
    }

    /// Returns the synchronized time from the server in milliseconds.
    public double GetRoundtripTime
    {
        get{ return _roomController.GetRoundtripTime; }
    }

    ///     Returns the synchronized time from the server in seconds.
    public double GetServerTimeSeconds
    {
        get { return _roomController.GetServerTimeSeconds; }
    }

    /// Returns a reference to the current networked user.
    public NetworkedUser CurrentUser
    {
        get{ return _roomController.CurrentNetworkedUser; }
    }

    public static bool IsReady
    {
        get{ return Instance != null; }
    }

    public bool IsInRoom
    {
        get { return _roomController.Room != null; }
    }

    private string userName;

    /// Display name for the user
    public string UserName
    {
        get{ return userName; }
        set{ userName = value; }
    }

    protected override void Start()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
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

        /// Set up Networked Entity Factory
        _networkedEntityFactory = new NetworkedEntityFactory(_roomController.CreationCallbacks, 
            _roomController.Entities, _roomController.EntityViews);
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
        StoneAgeRoomAvailable[] rooms = await client.GetAvailableRooms<StoneAgeRoomAvailable>(_roomController.roomName);

        onRoomsReceived?.Invoke(rooms);
    }

    public async void JoinExistingRoom(string roomID)
    {
        await _roomController.JoinRoomId(roomID);
    }

    public async void CreateNewRoom(string roomID, Dictionary<string, object> roomOptions)
    {
        _roomController.SetRoomOptions(roomOptions);
        
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

    public void AddEntityView(string entityId, NetworkedEntityView view)
    {
        _roomController.EntityViews.Add(entityId, view);
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
        RFCTargets target)
    {
        RFC(entity.Id, function, param, target);
    }

    /// Send a Remote Function Call
    /// The ID of the entity we want to send the RFC
    /// The name of the function to call
    /// The parameters of the function to call
    /// Who should receive this RFC
    public static void RFC(string entityId, string function, object[] param,
        RFCTargets target)
    {
        NetSend("remoteFunctionCall",
            new RFCMessage {entityId = entityId, function = function, param = param, target = target});
    }

    public static void CustomServerMethod(string methodName, object[] param)
    {
        NetSend("customMethod", new CustomMethodMessage {method = methodName, param = param});
    }

    public void SendObjectInit(Gatherable gatherable)
    {
        //LSLog.Log("Initializing " + gatherable.ID);
        NetSend("objectInit", new object[] {gatherable.ID, gatherable.GetServerType()});
    }

    public void SendObjectInit(Scorable scorable, float xPos, float yPos, int teamIndex)
    {
        //LSLog.Log("Initializing " + scorable.ID);
        NetSend("objectInit", new object[] {scorable.ID, scorable.GetServerType(), xPos, yPos, teamIndex});
    }

    public void SetObjectOwner(Scorable scorable)
    {
        NetSend("objectOwner", new object[] {scorable.ID, CurrentUser.sessionId});
    }

    /// <summary>
    /// Sends a message to the room on the server that an <see cref="Gatherable"/> has been used
    /// </summary>
    /// <param name="gatherable">The <see cref="Gatherable"/> used.</param>
    /// <param name="entity">The entity that has used the <see cref="Gatherable"/>.</param>
    public void SendObjectGather(Gatherable gatherable, NetworkedEntityView entity)
    {
        //LSLog.Log("Sending object interaction for ID " + gatherable.ID);
        NetSend("objectGathered", new object[] {gatherable.ID, entity.Id, gatherable.GetServerType()});
    }
    /// <summary>
    /// Sends a message to the room on the server that an <see cref="Scorable"/> has been used
    /// </summary>
    /// <param name="scorable">The <see cref="Scorable"/> used.</param>
    /// <param name="entity">The entity that has used the <see cref="Scorable"/>.</param>
    public void SendObjectScore(Scorable scorable, NetworkedEntityView entity)
    {
        LSLog.Log("Sending object scoring for ID " + scorable.ID);
        NetSend("scoreChange", new object[] {scorable.ID, scorable.GetServerType()});
    }

    public void ClearCollectionsAndUser()
    {
        _roomController.ClearCollectionsAndUser();
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
}
