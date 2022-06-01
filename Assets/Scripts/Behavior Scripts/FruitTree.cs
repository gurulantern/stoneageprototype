using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FruitTree : Gatherable
{
    [SerializeField] FoodSource _fruitTreeRef;
    [SerializeField] private int harvestTrigger;
    [SerializeField] private int foodRemaining;
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        foodRemaining = _fruitTreeRef.foodTotal; 
        harvestTrigger = _fruitTreeRef.harvestTriggerInt;
    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    public void Harvest()
    {
        Debug.Log("Harvest is firing with playerNear = " + playerNear + " FoodRemaining = " + foodRemaining);
        if (playerNear == true && foodRemaining > 1) {
            gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
            harvestTrigger ++;
            foodRemaining = Mathf.Clamp(foodRemaining - _fruitTreeRef.foodTaken, 0, _fruitTreeRef.foodTotal);
            Debug.Log("Gathered invoked and foodRemaining = " + foodRemaining);
        } else if (playerNear == true && foodRemaining == 1) {
            gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
            harvestTrigger ++;
            foodRemaining = Mathf.Clamp(foodRemaining - _fruitTreeRef.foodTaken, 0, _fruitTreeRef.foodTotal);
            gameObject.tag = "Tree";       
        }
    }

}
