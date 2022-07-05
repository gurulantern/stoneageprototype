using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Gatherable
{
    protected override void Awake()
    {
        base.Awake();
        resourceTotal = _resourceRef.WoodTotal;
        resourceRemaining = _resourceRef.WoodTotal; 
        harvestTrigger = _resourceRef.HarvestTriggerInt;
        resourceTaken = _resourceRef.WoodTaken;
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    protected override void Harvest()
    {
        base.Harvest();
    }
}
