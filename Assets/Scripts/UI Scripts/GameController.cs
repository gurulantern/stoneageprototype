using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Colyseus;
using LucidSightTools;

public class GameController : MonoBehaviour
{
    public StoneColyseusNetworkedEntityView prefab;
    public CharController controls;
    public static GameController instance;
    public GameObject hudContainer, gameOverPanel;
    public TextMeshProUGUI allianceTracker, foodCounter;
    public float timeLimit = 60f;
    private float remainingTime;
    public bool gamePlaying { get; private set; }
    private int numTotalFood;

    private void Awake() 
    {
        instance = this;
    }
    private void Start() 
    {
        numTotalFood = 0;
        gamePlaying = false;
        remainingTime = timeLimit;
    }

    private void OnEnable() 
    {
        RoomController.onAddNetworkEntity += OnNetworkAdd;
        RoomController.onRemoveNetworkEntity += OnNetworkRemove;
    }

    private void OnNetworkAdd(NetworkedEntity entity)
    {
        if (ColyseusManager.Instance.HasEntityView(entity.id))
        {
            LSLog.LogImportant("View found! For " + entity.id);
        }
        else
        {
            LSLog.LogImportant("No View found for " + entity.id);
        }
    }

    private void OnNetworkRemove(NetworkedEntity entity, StoneColyseusNetworkedEntityView view)
    {
        RemoveView(view);
    }
    
    private void CreateView(NetworkedEntity entity)
    {
        LSLog.LogImportant("print: " + JsonUtility.ToJson(entity));
        StoneColyseusNetworkedEntityView newView = Instantiate(prefab);
        ColyseusManager.Instance.RegisterNetworkedEntityView(entity, newView);
        newView.gameObject.SetActive(true);
    }

    private void RemoveView(StoneColyseusNetworkedEntityView view)
    {
        view.SendMessage("OnEntityRemoved", SendMessageOptions.DontRequireReceiver);
    }
    private void FixedUpdate()
    {
        if (remainingTime > 0 && gamePlaying == true)
        {
            remainingTime = Mathf.Clamp(remainingTime - Time.fixedDeltaTime, 0, timeLimit);
            Timer.instance.DecrementTime(remainingTime / timeLimit);
        } else if (remainingTime == 0) {
            EndGame();
        }
    }

    public void BeginGame()
    {
        gamePlaying = true;
        Time.timeScale = 1.0f;
    }

    private void EndGame()
    {
        gamePlaying = false;
        Invoke("ShowGameOverScreen", 0f);
        Time.timeScale = 0f;
    }

    private void ShowGameOverScreen()
    {
        hudContainer.SetActive(false);
        gameOverPanel.SetActive(true);
    }
}