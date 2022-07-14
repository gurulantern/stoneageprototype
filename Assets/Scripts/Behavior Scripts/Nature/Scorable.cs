using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using LucidSightTools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// The base representation of a server-scorable object.
/// An object placed in a grid position that can be interacted with by players and are linked to a schema state on the server side
/// </summary>
public abstract class Scorable : Interactable
{
    public string requiredResource;
    
    /// <summary>
    /// The schema state provided from the server
    /// </summary>
    protected ScorableState _state;

    public ScorableState State
    {
        get
        {
            return _state;
        }
    }

    protected string clickedTag;


    protected virtual void Awake() {

    }

    private void Start() {
    }


        /// <summary>
    /// Hand off the <see cref="InteractableState"/> from the server
    /// </summary>
    /// <param name="state"></param>
    public void SetState(ScorableState state)
    {
        _state = state;
        _state.OnChange += OnStateChange;
        //UpdateForState();
    }

    public override void PlayerAttemptedUse(NetworkedEntityView entity)
    {
        base.PlayerAttemptedUse(entity);
        //Tell the server that this entity is attempting to use this interactable
        ColyseusManager.Instance.SendObjectScore(this, entity);
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
    /// Event handler for state changes
    /// </summary>
    /// <param name="changes"></param>
    protected virtual void OnStateChange(List<DataChange> changes)
    {
        //UpdateForState();
    }

    /// <summary>
    /// Arranges the object based off of it's current state
    /// </summary>
/*
    protected virtual void UpdateForState()
    {
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
    }
*/
}
