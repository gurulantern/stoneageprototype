using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using LucidSightTools;
using TMPro;
using UnityEngine;


/// <summary>
/// The base representation of a server-interactable object.
/// An object placed in a grid position that can be interacted with by players and are linked to a schema state on the server side
/// </summary>
public abstract class Gatherable : Interactable
{
    public string type;
    [SerializeField] protected int startingHarvest;
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


    private void Awake() 
    {
        if (ColyseusManager.Instance.IsInRoom) 
        {
            Debug.Log("Initializing a new Gatherable");
            InitializeSelf();
        }
    }

    /// <summary>
    /// Hand off the <see cref="GatherableState"/> from the server
    /// </summary>
    /// <param name="state"></param>
    public virtual void SetState(GatherableState state)
    {
        _state = state;
        _state.OnChange += OnStateChange;
        currHarvestTrigger = (int)_state.harvestTrigger;
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
        _state.harvestTrigger = (float)currHarvestTrigger; 
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
        currHarvestTrigger = (int)_state.harvestTrigger;
        prevHarvestTrigger = currHarvestTrigger + 1;
    }
    public virtual void PlayerAttemptedUse(NetworkedEntityView entity)
    {
        //Tell the server that this entity is attempting to use this interactable
        ColyseusManager.Instance.SendObjectGather(this, entity);
    }

    public void InitializeSelf() 
    {
        switch(this.gameObject.tag) {
            case "Fruit_Tree":
                this.SetID(EnvironmentController.Instance.fruitTreeCount += 1);
                break;
            case "Fruit":
                this.SetID(EnvironmentController.Instance.fruitCount += 1);
                break;
            case "Tree":
                this.SetID(EnvironmentController.Instance.treeCount += 1);
                break;
            case "Aurochs":
                this.SetID(EnvironmentController.Instance.aurochsCount += 1);
                break;
        }
        ColyseusManager.Instance.SendObjectInit(this);
    }

    public override void OnSuccessfulUse(CharControllerMulti entity, string type)
    {
        base.OnSuccessfulUse(entity, type);
        //currHarvestTrigger = harvest;
        switch(type) {
            case "FRUIT":
                this.gameObject.GetComponent<Food>().Harvest();
                if(entity != null)
                {
                    entity.ChangeStamina(this.gameObject.GetComponent<Food>().restoreAmount);
                }
                break;
            case "TREE":
                this.gameObject.GetComponent<Tree>().Harvest();
                break;
            case "FRUIT_TREE":
                this.gameObject.GetComponent<FruitTree>().Harvest();
                break;
            case "AUROCHS":
                this.gameObject.GetComponent<Aurochs>().Harvest();
                break;
        }
        if (entity != null && type != "FRUIT") {
            entity.gameObject.GetComponent<CharControllerMulti>().StartGather(true);
        }
    }
}

