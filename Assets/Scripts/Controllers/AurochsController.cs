using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LucidSightTools;

public class AurochsController : MonoBehaviour
{

    private static AurochsController instance;

    public static AurochsController Instance
    {
        get
        {
            if (instance == null)
            {
                LSLog.LogError("No Aurochs Controller in scene!");
            }

            return instance;
        }
    }
    public GameObject liveAurochs;
    public GameObject deadAurochs;
    public int currentSpawn;

    public SpawnPoint[] aurochsSpawnPoints;
    public SpawnPoint[] deadAurochsSpawnPounts;

    private void Awake()
    {
        currentSpawn = 0;
        instance = this;
    }
    public void SpawnAurochs(bool alive, int spawnPoint)
    {
        if (alive)
        {
            Instantiate(liveAurochs, aurochsSpawnPoints[spawnPoint].transform);
        } else {
            Instantiate(deadAurochs, deadAurochsSpawnPounts[spawnPoint].transform);
        }
    }
}
