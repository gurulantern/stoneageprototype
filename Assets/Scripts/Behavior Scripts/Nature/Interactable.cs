using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;
using UnityEngine.Events;


public class Interactable : MonoBehaviour
{
    [SerializeField]
    private UnityEvent OnSuccessfulUseEvent;

    /// <summary>
    /// This must be unique from all other interactables in the grid space, as this is how the interactable will know which schema it is linked to
    /// </summary>
    [SerializeField]
    protected string _itemID;

    /// <summary>
    /// Public ID getter
    /// </summary>
    public string ID
    {
        get { return _itemID; }
    }

    
    /// <summary>
    /// When alerting the server of this interactable, some values will be initialized based on the serverType, namely cost and use durations
    /// </summary>
    [SerializeField]
    protected string serverType = "DEFAULT";

    /// <summary>
    /// Flag to tell local entities whether or not they can attempt to use this object
    /// </summary>
    protected bool isInUse;

    /// <summary>
    /// Array of <see cref="InteractableTrigger"/> colliders that a user can enter to initialize an interaction with this object
    /// </summary>
    [SerializeField]
    protected InteractableTrigger trigger;

    protected bool playerNear; 

        /// <summary>
    /// Set <see cref="isInUse"/> when the <see cref="InteractableState"/> changes
    /// </summary>
    /// <param name="inUse"></param>
    public virtual void SetInUse(bool inUse)
    {
        //Sanity check to make sure an object isn't double-used
        if (isInUse == inUse)
        {
            LSLog.LogError(string.Format("Tried to set Interactable {0}'s isInUse to {1} when it already was!", ID, isInUse));
        }

        isInUse = inUse;


        //trigger[i].enabled = !isInUse;
    }

    /// <summary>
    /// Override-able getter to determine if an Interactable is currently in use or not
    /// </summary>
    /// <returns></returns>
    public virtual bool InUse()
    {
        return isInUse;
    }

    /// <summary>
    /// Fired off by an <see cref="InteractableTrigger"/>. Alerts the interactable that it has a <see cref="NetworkedEntity"/> within range. Also tells the entity that it is within range of an interactable
    /// </summary>
    /// <param name="entity"></param>
    public virtual void PlayerInRange(CharControllerMulti entity)
    {
        if (InUse())
            return;

        entity.EntityNearInteractable(this);
        Debug.Log($"Player is near {this}");
        ///DisplayInRangeMessage();
    }

    /// <summary>
    /// Fired off by an <see cref="InteractableTrigger"/>. Alerts the interactable that a <see cref="NetworkedEntity"/> has exited it's range. Also tells the entity that it is no longer within range of an interactable
    /// </summary>
    /// <param name="entity"></param>
    public virtual void PlayerLeftRange(CharControllerMulti entity)
    {
        entity.EntityLeftInteractable(this);
        Debug.Log($"Player left {this}");
        ///HideInRangeMessage();
    }

    /// <summary>
    /// Sent by the <see cref="EnvironmentController"/> after the server sends a <see cref="ObjectUseMessage"/>
    /// </summary>
    /// <param name="entity"></param>
    public virtual void OnSuccessfulUse(CharControllerMulti entity, string type, int harvest)
    {
        OnSuccessfulUseEvent?.Invoke();
    }
/*
    /// <summary>
    /// Overrideable in case we want an interactable to do more than just show instructions when a player enters range
    /// </summary>
    protected virtual void DisplayInRangeMessage()
    {
        if (instructionRoot)
        {
            instructionRoot.SetActive(true);
        }
    }

    /// <summary>
    /// Overrideable in case we want an interactable to do more than just hide instructions when a player exits range
    /// </summary>
    protected virtual void HideInRangeMessage()
    {
        if (instructionRoot)
        {
            instructionRoot.SetActive(false);
        }
    }
*/
    /// <summary>
    /// Get the server type to initialize the server provided values
    /// </summary>
    /// <returns></returns>
    public string GetServerType()
    {
        //Has not been overriden, return default!
        return string.IsNullOrEmpty(serverType) ? "DEFAULT" : serverType;
    }
}
