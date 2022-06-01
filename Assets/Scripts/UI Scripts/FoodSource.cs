using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Food Source", fileName = "New Food Source", order = 0)]
public class FoodSource : ScriptableObject
{
    public int foodTotal = 7;
    public int harvestTriggerInt = 1;
    public int foodTaken = 1;
}
