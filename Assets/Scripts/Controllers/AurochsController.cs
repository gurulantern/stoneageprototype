using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LucidSightTools;

public class AurochsController : MonoBehaviour
{

    private static AurochsController instance;

    [SerializeField]
    private EnvironmentController _environmentController;

    private GameObject aurochs;
    private Quaternion blankRot = new Quaternion(0, 0, 0, 0);

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
            aurochs = (GameObject)Instantiate(liveAurochs, aurochsSpawnPoints[spawnPoint].transform.position, blankRot, _environmentController.gameObject.transform);
        } else {
            aurochs = (GameObject)Instantiate(deadAurochs, deadAurochsSpawnPounts[spawnPoint].transform.position, blankRot, _environmentController.gameObject.transform);
        }
        Aurochs aurochsScript = aurochs.GetComponent<Aurochs>();
        aurochsScript.InitializeSelf();
    }
}
