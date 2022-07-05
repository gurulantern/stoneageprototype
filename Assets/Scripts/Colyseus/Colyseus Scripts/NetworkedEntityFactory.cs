using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using GameDevWare.Serialization;
using LucidSightTools;

/// Responsible for carrying out the creation of network entities and registering them with the Colyseus Manager.
public class NetworkedEntityFactory
{
    private readonly Dictionary<string, Action<NetworkedEntity>> _creationCallbacks;
    // TODO: replace GameDevWare stuff
    private readonly IndexedDictionary<string, NetworkedEntity> _entities;
    private readonly IndexedDictionary<string, NetworkedEntityView> _entityViews;

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
    public void CreateNetworkedEntityWithTransform(ColyseusRoom<RoomState> room, Vector3 position,
        Dictionary<string, object> attributes = null, StoneColyseusNetworkedEntityView viewToAssign = null,
        Action<NetworkedEntity> callback = null)
    {
        if (attributes != null)
        {
            attributes.Add("creationPos", new object[2] { position.x, position.y, });
            ///attributes.Add("prefab",prefab);
        }
        else
        {
            attributes = new Dictionary<string, object>()
            {
                ["creationPos"] = new object[2] { position.x, position.y },
                ///["prefab"] = new object[1] {prefab}
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
            Debug.Log("InititView");
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
}
