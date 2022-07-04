using UnityEngine;

public class FruitTree : Gatherable
{
    [SerializeField] Source _fruitTreeRef;
    [SerializeField] private int foodTotal;
    [SerializeField] private int harvestTrigger;
    [SerializeField] private int foodTaken;
    [SerializeField] private int foodRemaining;
    Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        foodTotal = _fruitTreeRef.FoodTotal;
        foodRemaining = _fruitTreeRef.FoodTotal; 
        harvestTrigger = _fruitTreeRef.HarvestTriggerInt;
        foodTaken = _fruitTreeRef.FoodTaken;
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    public void Harvest()
    {
        Debug.Log("Harvest is firing with playerNear = " + playerNear + " FoodRemaining = " + foodRemaining);
        if (playerNear == true && foodRemaining > 1) {
            gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
            harvestTrigger ++;
            foodRemaining = Mathf.Clamp(foodRemaining - foodTaken, 0, foodTotal);
            Debug.Log("Gathered invoked and foodRemaining = " + foodRemaining);
        } else if (playerNear == true && foodRemaining == 1) {
            gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
            harvestTrigger ++;
            foodRemaining = Mathf.Clamp(foodRemaining - foodTaken, 0, foodTotal);
            gameObject.tag = "Tree";       
        }
    }

}
