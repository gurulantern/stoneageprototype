using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using LucidSightTools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// The base representation of a server-interactable object.
/// An object placed in a grid position that can be interacted with by players and are linked to a schema state on the server side
/// </summary>
public abstract class Gatherable : Interactable
{
    [SerializeField] protected int prevHarvestTrigger;
    [SerializeField] protected int currHarvestTrigger;

    /// <summary>
    /// The schema state provided from the server
    /// </summary>
    protected GatherableState _state;

    public GatherableState State
    {
        get
        {
            return _state;
        }
    }
    protected GatherableState previousState;
    protected GatherableState localUpdatedState;
    protected string clickedTag;

    protected override void Awake() {
        base.Awake();
    }
/*
    protected virtual void Harvest()
    {

    }
*/
        /// <summary>
    /// Hand off the <see cref="GatherableState"/> from the server
    /// </summary>
    /// <param name="state"></param>
    public virtual void SetState(GatherableState state)
    {
        _state = state;
        _state.OnChange += OnStateChange;
        prevHarvestTrigger = (int)_state.harvestTrigger;
        UpdateStateForView();
    } 

    public void SetID(int num)
    {
        _itemID = $"{gameObject.tag}_{num}";
    }


    /// <summary>
    /// Clean-up delegates
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_state != null) //This object has been given a state
        {
            _state.OnChange -= OnStateChange;
        }
    }

    /// <summary>
    /// Updates the state for other player views
    /// </summary>
    protected virtual void UpdateStateForView()
    {

    }

    /// <summary>
    /// Event handler for state changes
    /// </summary>
    /// <param name="changes"></param>
    protected virtual void OnStateChange(List<DataChange> changes)
    {
        UpdateViewFromState();
    }

    /// <summary>
    /// Arranges the object based off of it's current state
    /// </summary>
    protected virtual void UpdateViewFromState()
    {
        /*
        //The current in use status is not what the State indicates
        if (isInUse != State.inUse)
        {
            if (isInUse && !State.inUse)
            {
                //Was previously in use but not anymore!
                OnInteractableReset();
            }
            //Set the interactable's inUse status
            SetInUse(State.inUse);
        }
        */
        
    }
    public override void PlayerAttemptedUse(NetworkedEntityView entity)
    {
        base.PlayerAttemptedUse(entity);
        //Tell the server that this entity is attempting to use this interactable
        ColyseusManager.Instance.SendObjectGather(this, entity);
    }

    public override void OnSuccessfulUse(CharControllerMulti entity, string type, int harvest)
    {
        base.OnSuccessfulUse(entity, type, harvest);
        currHarvestTrigger = harvest;
        switch(type) {
            case "TREE":
                this.gameObject.GetComponent<Tree>().Harvest();
                break;
            case "FRUIT_TREE":
                this.gameObject.GetComponent<FruitTree>().Harvest();
                break;
            case "DEAD_AUROCHS":
                this.gameObject.GetComponent<Aurochs>().Harvest();
                break;
            case "LIVE_AUROCHS":
                this.gameObject.GetComponent<Aurochs>().Harvest();
                break;
        }
        entity.gameObject.GetComponent<CharControllerMulti>().StartGather();
    }
}
