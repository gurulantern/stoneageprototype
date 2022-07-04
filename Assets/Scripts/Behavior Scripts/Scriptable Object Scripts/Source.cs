using UnityEngine;

[CreateAssetMenu(menuName = "Source", fileName = "New Source", order = 0)]
public class Source : ScriptableObject
{
    [SerializeField] private int foodTotal = 7;
    public int FoodTotal 
    {
        get { return foodTotal; }
    }

    [SerializeField] private int woodTotal = 10;
    public int WoodTotal
    {
        get { return woodTotal; }
    }

    [SerializeField] private int seedTotal = 20;
    public int SeedTotal
    {
        get { return seedTotal; }
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

    [SerializeField] private int woodTaken = 1;
    public int WoodTaken
    {
        get { return woodTaken; }
    }

    [SerializeField] private int seedTaken = 1;
    public int SeedTaken
    {
        get { return seedTaken; }
    }
}
