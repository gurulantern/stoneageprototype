using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

[CreateAssetMenu(menuName = "FoodCollection", fileName = "New Food Collection", order = 0)]
public class FoodCollection : ScriptableObject 
{

    public int foodCount = 0;
    public event Action Changed;
    void Awake()
    {
        foodCount = 0;
    }
    public void AddFood(int food) 
    {
        foodCount += food;
        Changed?.Invoke();
    }

    public void TakeFood()
    {
       foodCount -= 1;
       Changed?.Invoke();
    }


}
