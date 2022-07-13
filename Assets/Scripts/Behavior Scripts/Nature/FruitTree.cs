using UnityEngine;

public class FruitTree : Tree
{
    protected override void Awake()
    {
        base.Awake();
    }
    //Left click decreases food remaining and triggers the animation for food to disappear

    public override void SetState(GatherableState state)
    {
        base.SetState(state);
    }
    public override void Harvest()
    {
        _treeStates[prevHarvestTrigger].gameObject.SetActive(false);
        _treeStates[currHarvestTrigger].gameObject.SetActive(true);
        prevHarvestTrigger = currHarvestTrigger;
        if (prevHarvestTrigger == 8)
        {
            this.gameObject.tag = "Tree";
        } else if (prevHarvestTrigger == 1) {
            this.gameObject.GetComponent<BoxCollider2D>().offset = colliderOffset;
            this.gameObject.GetComponent<BoxCollider2D>().size = colliderSize;
        }
    }

    protected override void UpdateStateForView()
    {
        //base.UpdateStateForView();

    }
}
