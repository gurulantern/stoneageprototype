using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Gatherable
{
    protected override void Awake()
    {
        base.Awake();
        resourceTotal =   (int)_state.woodTotal;
        resourceRemaining =   (int)_state.woodTotal; 
        harvestTrigger =   (int)_state.harvestTrigger;
        resourceTaken =  (int)_state.resourceTaken;
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    protected override void Harvest()
    {
        base.Harvest();
    }
}
