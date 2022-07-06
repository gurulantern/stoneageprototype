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




    [SerializeField] protected Source _resourceRef;
    [SerializeField] protected int resourceTotal;
    [SerializeField] protected int harvestTrigger;
    [SerializeField] protected int resourceTaken;
    [SerializeField] protected int resourceRemaining;
    protected int i;
    protected bool playerNear; 
    protected string clickedTag;
    Animator animator;

    protected virtual void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        i = 0;
    }
    protected void OnTriggerEnter2D(Collider2D other) {
        
        if (other.gameObject.GetComponent<CharControllerMulti>().IsMine && i == 0) {
            playerNear = true;
            i++;
        } else {
            Debug.Log(other.gameObject.tag);
        }
    }

    protected void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.GetComponent<CharControllerMulti>().IsMine && i == 1) {
            playerNear = false;
            i = 0 ;
        }
    }

    protected virtual void Harvest()
    {
        Debug.Log("Harvest is firing with playerNear = " + playerNear + " ResourceRemaining = " + resourceRemaining);
        if (playerNear == true && resourceRemaining > 0) {
            gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
            harvestTrigger ++;
            resourceRemaining = Mathf.Clamp(resourceRemaining - resourceTaken, 0, resourceTotal);
            Debug.Log("Gathered invoked and foodRemaining = " + resourceRemaining);
        }
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
