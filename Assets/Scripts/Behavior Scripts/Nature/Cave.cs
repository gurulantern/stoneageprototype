using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Cave : Scorable
{
    public SpawnPoint[] spawnPoints;
    public Light fire;
    public SpriteRenderer family;
    public SpriteRenderer fireLog;

    protected virtual void Awake()
    {
        requiredResource = "food";
    }

    public void SetAsHome(bool isHome)
    {
        family.gameObject.SetActive(isHome);
        fireLog.gameObject.SetActive(isHome);
        fire.gameObject.SetActive(isHome);
    }
    
}
