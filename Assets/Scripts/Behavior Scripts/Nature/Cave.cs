using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Cave : Gatherable
{
    [SerializeField] private FoodCollection _foodCollection;
    public int teamIndex;
    public SpawnPoint[] spawnPoints;
    private int foodCount;
    public int FoodCount { get; }
    public event Action Changed;

    void Awake()
    {
        foodCount = _foodCollection.FoodCount;
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
