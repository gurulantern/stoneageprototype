using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class FruitTree : Gatherable
{
    public int foodTotal = 7;
    private int harvestTriggerInt;
    private int foodRem;
    public int FoodRem { get; set;}
    private int food = 1;
    GameObject fruitTree;

    public override void Gather()
    {
        throw new System.NotImplementedException();
    }
    void Start()
    {
        fruitTree = GetComponent<GameObject>();
        FoodRem = foodTotal; 
        harvestTriggerInt = 1;
    }

    //Left click decreases food remaining and triggers the animation for food to disappear
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && playerNear == true && FoodRem > 0) {
            base.OnPointerClick(eventData);
            animator.SetTrigger("Harvest " + harvestTriggerInt.ToString());
            harvestTriggerInt ++;
            FoodRem = Mathf.Clamp(FoodRem - food, 0, foodTotal);
            Debug.Log("Food Remaining: " + FoodRem);    
        }
    }

}
