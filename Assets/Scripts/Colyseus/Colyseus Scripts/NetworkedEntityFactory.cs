using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using GameDevWare.Serialization;
using LucidSightTools;

/// Responsible for carrying out the creation of network entities and registering them with the Colyseus Manager.
public class NetworkedEntityFactory: MonoBehaviour
{
    private static NetworkedEntityFactory instance;
    public static NetworkedEntityFactory Instance
    {
        get 
        {
            if (instance == null)
            {
                LSLog.LogError("No NetworkedEntityFactory in scene!");
            }
            return instance;
        }
    }

    [SerializeField]
    private GameObject entityPrefab = null;
    [SerializeField]
    private Dictionary<string, NetworkedEntity> entities = new Dictionary<string, NetworkedEntity>();
    public CameraBehavior cameraController;
    private string _ourEntityId;
    private readonly Dictionary<string, Action<NetworkedEntity>> _creationCallbacks;
    // TODO: replace GameDevWare stuff
    private readonly IndexedDictionary<string, NetworkedEntity> _entities;
    private readonly IndexedDictionary<string, NetworkedEntityView> _entityViews;

    void Awake()
    {
        instance = this;
    }

    public void SetCameraTarget(Transform target)
    {
        cameraController.SetFollow(target);
    }

    public void SpawnEntity(NetworkedEntityState state, bool isPlayer = false)
    {
        Debug.Log("Spawning Entity");
        Vector3 position = new Vector3((float)state.xPos, (float)state.yPos, (float)state.zPos);
        Quaternion rot =  new Quaternion((float)state.xRot, (float)state.yRot, (float)state.zRot, 1.0f);
/// Possible place to set the spawn point
        GameObject newEntity = Instantiate(entityPrefab, position, rot);

        newEntity.transform.SetParent(GameObject.Find("Grid").transform);
        newEntity.name = ColyseusManager.Instance.UserName;
        NetworkedEntity entity = newEntity.GetComponent<NetworkedEntity>();
        entity.Initialize(state, isPlayer);
        entities.Add(state.id, entity);

        Debug.Log("Setting Camera transform to entity transform");
        if (isPlayer)
        {
            _ourEntityId = state.id;

            SetCameraTarget(newEntity.transform);
            newEntity.GetComponent<CharControllerMulti>().cameraTransform = cameraController.transform;
        }
    }

    public bool UpdateOurEntity(NetworkedEntityState state)
    {
        if (entities.ContainsKey(_ourEntityId))
        {
            NetworkedEntity entity = entities[_ourEntityId];
            entities.Remove(_ourEntityId);
            entity.Initialize(state, true);
            _ourEntityId = state.id;
            entities.Add(_ourEntityId, entity);
            return true;
        }

        LSLog.LogError($"Missing our entity? - \"{_ourEntityId}\"");
        return false;
    }

        /// <summary>
    /// Removes the entity, keyed by session Id, from the controlled entities and
    /// destroys the player game object.
    /// </summary>
    /// <param name="id"></param>
    public void RemoveEntity(string id)
    {
        if (entities.ContainsKey(id))
        {
            NetworkedEntity entity = entities[id];
            entities.Remove(id);
            Destroy(entity.gameObject);
        }
    }

    /// <summary>
    /// Returns the <see cref="NetworkedEntity"/> belonging to this client.
    /// </summary>
    /// <returns></returns>
    public NetworkedEntity GetMine()
    {
        foreach (KeyValuePair<string, NetworkedEntity> entry in entities)
        {
            if (entry.Value.isMine)
            {
                return entry.Value;
            }
        }

        LSLog.LogError("No entity found for user!");
        return null;
    }

    /// <summary>
    /// Returns the <see cref="NetworkedEntity"/> belonging to the given session Id if one exists.
    /// </summary>
    /// <param name="sessionId">The session Id of the desired <see cref="NetworkedEntity"/></param>
    /// <returns></returns>
    public NetworkedEntity GetEntityByID(string sessionId)
    {
        if (entities.ContainsKey(sessionId))
        {
            return entities[sessionId];
        }

        return null;
    }

    /// <summary>
    /// Clears the collection of controlled <see cref="NetworkedEntity"/>s and destroys all the linked player game objects.
    /// </summary>
    /// <param name="excludeOurs">If true the <see cref="NetworkedEntity"/> and player game object belonging to this client will not be removed and destroyed.</param>
    public void RemoveAllEntities(bool excludeOurs)
    {
        List<string> keys = new List<string>(entities.Keys);

        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (entities[keys[i]].isMine && excludeOurs)
            {
                continue;
            }

            Destroy(entities[keys[i]].gameObject);

            entities.Remove(keys[i]);
        }
    }
/*
    public NetworkedEntityFactory(Dictionary<string, Action<NetworkedEntity>> creationCallbacks, IndexedDictionary<string, NetworkedEntity> entities, IndexedDictionary<string, NetworkedEntityView> entityViews)
    {
        _creationCallbacks = creationCallbacks;
        _entities = entities;
        _entityViews = entityViews;
    }

    /// Creates a new NetworkedEntity with the given prefab and attributes
    /// and places it at the provided position and rotation.
    /// room - The room the entity will be added to
    /// prefab - Prefab you would like to use to create the entity
    /// position - Position for the new entity
    /// rotation - Position for the new entity
    /// attributes - Position for the new entity
    public void InstantiateNetworkedEntity(ColyseusRoom<RoomState> room, string prefab, Vector2 position,
        Dictionary<string, object> attributes = null)
    {
        if (string.IsNullOrEmpty(prefab))
        {
            LSLog.LogError("No Prefab Declared");
            return;
        }

        if (attributes != null)
        {
            attributes.Add("creationPos", new object[2] { position.x, position.y });
        }
        else
        {
            attributes = new Dictionary<string, object>()
            {
                ["creationPos"] = new object[2] { position.x, position.y },
            };
        }

        CreateNetworkedEntity(room, prefab, attributes);
    }

    /// Creates a new NetworkedEntity with the given prefab, attributes, and ColyseusNetworkedEntityView.
    /// room - The room the entity will be added to
    /// prefab - Prefab you would like to use
    /// attributes - Position for the new entity
    /// viewToAssign - The provided view that will be assigned to the new NetworkedEntity
    /// callback - Callback that will be invoked with the newly created NetworkedEntity
    public void CreateNetworkedEntity(ColyseusRoom<RoomState> room, string prefab, Dictionary<string, object> attributes = null, StoneColyseusNetworkedEntityView viewToAssign = null, Action<NetworkedEntity> callback = null)
    {
        Dictionary<string, object> updatedAttributes = (attributes != null)
            ? new Dictionary<string, object>(attributes)
            : new Dictionary<string, object>();
        updatedAttributes.Add("prefab", prefab);
        CreateNetworkedEntity(room, updatedAttributes, viewToAssign, callback);
    }

    /// Creates a new NetworkedEntity attributes and ColyseusNetworkedEntityView
    /// The room the entity will be added to
    /// Position for the new entity
    /// The provided view that will be assigned to the new NetworkedEntity
    /// Callback that will be invoked with the newly created NetworkedEntity
    public void CreateNetworkedEntity(ColyseusRoom<RoomState> room, Dictionary<string, object> attributes = null, StoneColyseusNetworkedEntityView viewToAssign = null, Action<NetworkedEntity> callback = null)
    {
        try
        {
            string creationId = null;

            if (viewToAssign != null || callback != null)
            {
                creationId = Guid.NewGuid().ToString();
                if (callback != null)
                {
                    if (viewToAssign != null)
                    {
                        _creationCallbacks.Add(creationId, (newEntity) =>
                        {
                            RegisterNetworkedEntityView(newEntity, viewToAssign);
                            callback.Invoke(newEntity);
                        });
                    }
                    else
                    {
                        _creationCallbacks.Add(creationId, callback);
                    }
                }
                else
                {
                    _creationCallbacks.Add(creationId,
                        (newEntity) => { RegisterNetworkedEntityView(newEntity, viewToAssign); });
                }
            }

            // send "createEntity" message for the server.
            LSLog.LogImportant("Sending server message to create Entity");
            _ = room.Send("createEntity", new { creationId = creationId, attributes = attributes });
        }
        catch (System.Exception err)
        {
            LSLog.LogError(err.Message + err.StackTrace);
        }

    }

    /// Creates a new NetworkedEntity with the given ColyseusNetworkedEntityView and attributes
    /// and places it at the provided position and rotation.
    /// room - The room the entity will be added to
    /// position - Position for the new entity
    /// rotation - Position for the new entity
    /// attributes - Position for the new entity
    /// viewToAssign - The provided view that will be assigned to the new NetworkedEntity
    /// name - callback">Callback that will be invoked with the newly created NetworkedEntity
    public void CreateNetworkedEntityWithTransform(ColyseusRoom<RoomState> room, Vector2 position,
        Dictionary<string, object> attributes = null, StoneColyseusNetworkedEntityView viewToAssign = null,
        Action<NetworkedEntity> callback = null)
    {
        if (attributes != null)
        {
            attributes.Add("creationPos", new object[2] { position.x, position.y, });
        }
        else
        {
            attributes = new Dictionary<string, object>()
            {
                ["creationPos"] = new object[2] { position.x, position.y },
            };
        }

        CreateNetworkedEntity(room, attributes, viewToAssign, callback);
    }

    /// Creates a GameObject using the NetworkedEntityView.
    /// Requires that the entity has a "prefab" attribute defined.
    public async Task CreateFromPrefab(NetworkedEntity entity)
    {
        LSLog.LogError($"Factory - Create From Prefab - {entity.id}");

        ResourceRequest asyncItem = Resources.LoadAsync<NetworkedEntityView>(entity.attributes["prefab"]);
        while (asyncItem.isDone == false)
        {
            await Task.Yield();
        }

        NetworkedEntityView view = UnityEngine.Object.Instantiate((NetworkedEntityView)asyncItem.asset);
        if (view == null)
        {
            LSLog.LogError("Instantiated Object is not of MultiPlayer Type");
            asyncItem = null;
            return;
        }

        RegisterNetworkedEntityView(entity, view);
    }

    /// Registers the NetworkedEntityView with the manager for tracking.
    /// Initializes the NetworkedEntityView if it has not yet been initialized.
    public void RegisterNetworkedEntityView(NetworkedEntity model, StoneColyseusNetworkedEntityView view)
    {
        if (string.IsNullOrEmpty(model.id) || view == null || _entities.ContainsKey(model.id) == false)
        {
            LSLog.LogError("Cannot Find Entity in Room");
            return;
        }

        NetworkedEntityView entityView = (NetworkedEntityView) view;

        if (entityView && !entityView.HasInit)
        {
            entityView.InitiView(model);
        }

        _entityViews.Add(model.id, (NetworkedEntityView)view);
        view.SendMessage("OnEntityViewRegistered", SendMessageOptions.DontRequireReceiver);
    }

    public void UnregisterNetworkedEntityView(NetworkedEntity model)
    {
        if (string.IsNullOrEmpty(model.id) || _entities.ContainsKey(model.id) == false)
        {
            LSLog.LogError("Cannot Find Entity in Room");
            return;
        }

        NetworkedEntityView view = _entityViews[model.id];

        _entityViews.Remove(model.id);
        view.SendMessage("OnEntityViewUnregistered", SendMessageOptions.DontRequireReceiver);
    }
*/
}
