using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Gatherable
{
    [SerializeField] protected SpriteRenderer[] _treeStates;
    protected override void Awake()
    {
        base.Awake();
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    protected virtual void Harvest()
    {
        _treeStates[prevHarvestTrigger].gameObject.SetActive(false);
        prevHarvestTrigger = (int)_state.harvestTrigger;
        _treeStates[prevHarvestTrigger].gameObject.SetActive(true);    }

    public override void SetState(GatherableState state)
    {
        base.SetState(state);
    }
}
