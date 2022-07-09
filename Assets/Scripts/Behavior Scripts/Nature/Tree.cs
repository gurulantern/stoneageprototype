using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Gatherable
{
    protected override void Awake()
    {
        base.Awake();
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    protected override void Harvest()
    {
        base.Harvest();
    }

    public override void SetState(GatherableState state)
    {
        base.SetState(state);
        resourceTotal =   (int)_state.woodTotal;
        resourceRemaining =   (int)_state.woodTotal; 
    }
}
