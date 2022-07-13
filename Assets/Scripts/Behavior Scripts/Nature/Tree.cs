using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Gatherable
{
    protected Vector2 colliderOffset; 
    protected Vector2 colliderSize;
    [SerializeField] protected SpriteRenderer[] _treeStates;
    protected override void Awake()
    {
        base.Awake();
        colliderOffset.x = -0.019f;
        colliderOffset.y = 0.216f;
        colliderSize.x = .827f;
        colliderSize.y = .397f;
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    public virtual void Harvest()
    {
        _treeStates[prevHarvestTrigger].gameObject.SetActive(false);
        _treeStates[currHarvestTrigger].gameObject.SetActive(true);
        prevHarvestTrigger = currHarvestTrigger; 
        if (prevHarvestTrigger == 1) {
            this.gameObject.GetComponent<BoxCollider2D>().offset = colliderOffset;
            this.gameObject.GetComponent<BoxCollider2D>().size = colliderSize;
        }
    }

    public override void SetState(GatherableState state)
    {
        base.SetState(state);
    }
}
