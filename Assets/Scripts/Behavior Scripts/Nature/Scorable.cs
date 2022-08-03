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
    [SerializeField] 
    protected GameObject[] states;
    public int ownerTeam;
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

    public void SetID(int num)
    {
        _itemID = $"{gameObject.tag}_{num}";
    }

    public void SetOwnerTeam(int num)
    {
        ownerTeam = num;
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

   protected virtual void UpdateViewFromState()
    {
        /*
        currHarvestTrigger = (int)_state.harvestTrigger;
        prevHarvestTrigger = currHarvestTrigger + 1;
        */
    }
    public virtual void PlayerAttemptedUse(NetworkedEntityView entity, int spend)
    {
        //Tell the server that this entity is attempting to use this interactable
        ColyseusManager.Instance.SendObjectScore(this, entity);
    }
/*
    public void InitializeSelf()
    {
        switch(this.gameObject.tag) {
            case "Aurochs_Pen":
                this.SetID(EnvironmentController.Instance.aurochsPen += 1);
                break;
            case "Farm":
                this.SetID(EnvironmentController.Instance.farms += 1);
                break;
            case "Sapling":
                this.SetID(EnvironmentController.Instance.saplings += 1);
                break;
            case "Fish_Trap":
                this.SetID(EnvironmentController.Instance.fishTraps += 1);
                break;
        }
        ColyseusManager.Instance.SendObjectInit(this, this.gameObject.transform.position.x, this.gameObject.transform.position.y);
    }
*/
    public override void OnSuccessfulUse(CharControllerMulti entity, string type)
    {
        base.OnSuccessfulUse(entity, type);
        //currHarvestTrigger = harvest;
        switch(type) {
            case "AUROCHS_PEN":
                this.gameObject.GetComponent<Food>().Harvest();
                if(entity != null)
                {
                    entity.ChangeStamina(this.gameObject.GetComponent<Food>().restoreAmount);
                }
                break;
            case "FARM":
                this.gameObject.GetComponent<Tree>().Harvest();
                break;
            case "FISH_TRAP":
                this.gameObject.GetComponent<Aurochs>().Harvest();
                break;
        }
        if (entity != null) {
            entity.gameObject.GetComponent<CharControllerMulti>().StartGather(true);
        }
    }
}

