using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tree : Gatherable
{
    protected Vector2 colliderOffset; 
    protected Vector2 colliderSize;
    [SerializeField] protected SpriteRenderer[] _treeStates;
    private void Awake()
    {
        startingHarvest = _treeStates.Length - 1; 
        colliderOffset.x = -0.019f;
        colliderOffset.y = 0.216f;
        colliderSize.x = .827f;
        colliderSize.y = .397f;
        this.gameObject.tag = "Tree";
        type = "Tree";
    }

    public override void SetState(GatherableState gatherable)
    {
        base.SetState(gatherable);
        if(currHarvestTrigger != startingHarvest) {
            _treeStates[startingHarvest].gameObject.SetActive(false);
        }
    }

    protected override void UpdateViewFromState()
    {
        base.UpdateViewFromState();
        _treeStates[currHarvestTrigger].gameObject.SetActive(true);
        _treeStates[prevHarvestTrigger].gameObject.SetActive(false);
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    public virtual void Harvest()
    {
        UpdateStateForView();
        if (prevHarvestTrigger == 1) {
            this.gameObject.GetComponent<BoxCollider2D>().offset = colliderOffset;
            this.gameObject.GetComponent<BoxCollider2D>().size = colliderSize;
        }
        //prevHarvestTrigger += 1;
    }

    protected override void UpdateStateForView()
    {
        base.UpdateStateForView();
        _treeStates[prevHarvestTrigger].gameObject.SetActive(false);
        _treeStates[currHarvestTrigger].gameObject.SetActive(true);
    }
}
