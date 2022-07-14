using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Cave : Scorable
{
    public int teamIndex;
    public SpawnPoint[] spawnPoints;

    void Awake()
    {
        //foodCount = _foodCollection.FoodCount;
    }
    
}
