using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aurochs : Gatherable
{
    [SerializeField] private bool alive;
    protected override void Awake()
    {
        base.Awake();
        resourceTotal = _resourceRef.FoodTotal;
        resourceRemaining = _resourceRef.FoodTotal; 
        harvestTrigger = _resourceRef.HarvestTriggerInt;
        resourceTaken = _resourceRef.FoodTaken;
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    protected override void Harvest()
    {
        base.Harvest();
    }
}
