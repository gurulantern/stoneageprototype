using UnityEngine;

public class FruitTree : Gatherable
{
    [SerializeField] FoodSource _fruitTreeRef;
    private int foodTotal;
    private int harvestTrigger;
    private int foodTaken;
    private int foodRemaining;
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
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
