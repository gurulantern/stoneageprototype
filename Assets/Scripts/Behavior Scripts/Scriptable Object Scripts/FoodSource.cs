using UnityEngine;

[CreateAssetMenu(menuName = "Food Source", fileName = "New Food Source", order = 0)]
public class FoodSource : ScriptableObject
{
    [SerializeField] private int foodTotal = 7;
    public int FoodTotal 
    {
        get { return foodTotal; }
    }

    [SerializeField] private int harvestTriggerInt = 1;
    public int HarvestTriggerInt
    {
        get { return harvestTriggerInt; }
    }
    [SerializeField] private int foodTaken = 1;
    public int FoodTaken
    {
        get { return foodTaken; }
    }
}
