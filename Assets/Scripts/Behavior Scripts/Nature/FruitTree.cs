using UnityEngine;

public class FruitTree : Tree
{
    [SerializeField] private int fruitTotal;
    [SerializeField] private int fruitTaken;
    [SerializeField] private int fruitRemaining;  
    [SerializeField] private int seedsTotal;
    [SerializeField] private int seedsTaken;
    [SerializeField] private int seedsRemaining;  

    protected override void Awake()
    {
        base.Awake();
        fruitTotal = _resourceRef.FoodTotal;
        fruitRemaining = _resourceRef.FoodTotal; 
        fruitTaken = _resourceRef.FoodTaken;
        seedsTotal = _resourceRef.SeedTotal;
        seedsTaken = _resourceRef.SeedTaken;
        seedsRemaining = _resourceRef.SeedTotal;
        harvestTrigger = _resourceRef.HarvestTriggerInt;
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    protected override void Harvest()
    {
        if (gameObject.tag == "Tree") {
            base.Harvest();
        } else {
            Debug.Log($"Harvest is firing with playerNear = {playerNear}, fruitRemaining = {fruitRemaining}, seedsRemaing = {seedsRemaining}");
            if (playerNear == true && fruitRemaining > 1) {
                gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
                harvestTrigger ++;
                fruitRemaining = Mathf.Clamp(fruitRemaining - fruitTaken, 0, fruitTotal);
                seedsRemaining = Mathf.Clamp(seedsRemaining - seedsTaken, 0, seedsTotal);
                Debug.Log($"Gathered invoked and fruitRemaining = {fruitRemaining}");
            } else if (playerNear == true && fruitRemaining == 1) {
                gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
                harvestTrigger ++;
                fruitRemaining = Mathf.Clamp(fruitRemaining - fruitTaken, 0, fruitTotal);
                seedsRemaining = Mathf.Clamp(seedsRemaining - seedsTaken, 0, seedsTotal);
                gameObject.tag = "Tree";       
            }
        }
    }

}
