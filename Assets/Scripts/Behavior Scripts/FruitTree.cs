using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FruitTree : Gatherable
{
    [SerializeField] GameEvent _gathered;
    [SerializeField] FoodSource _fruitTreeRef;
    [SerializeField] private int harvestTrigger;
    [SerializeField] private int foodRemaining;
    public int FoodRemaining { get; set;}
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        FoodRemaining = _fruitTreeRef.foodTotal; 
        harvestTrigger = _fruitTreeRef.harvestTriggerInt;

    }
    //Left click decreases food remaining and triggers the animation for food to disappear
    public void Harvest()
    {
        if (playerNear == true && FoodRemaining > 0) {
            gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
            harvestTrigger ++;
            FoodRemaining = Mathf.Clamp(FoodRemaining - _fruitTreeRef.foodTaken, 0, _fruitTreeRef.foodTotal);
            _gathered?.Invoke();
            Debug.Log("Food Remaining: " + FoodRemaining);    
        }
    }

}
