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
    public Light fire;
    public SpriteRenderer family;
    public SpriteRenderer fireLog;

    protected virtual void Awake()
    {
        requiredResource = "food";
    }
    
}
