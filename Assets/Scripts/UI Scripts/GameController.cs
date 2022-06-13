using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Colyseus;
using Colyseus.Schema;
using LucidSightTools;

public class GameController : MonoBehaviour
{
    public StoneColyseusNetworkedEntityView prefab;
    public delegate void OnViewAdded(StoneColyseusNetworkedEntityView view);
    public static event OnViewAdded onViewAdded;

    public delegate void OnViewRemoved(StoneColyseusNetworkedEntityView view);
    public static event OnViewRemoved onViewRemoved;
    public static GameController Instance { get; set; }
    CharController playerStats;
    [SerializeField] private Cave homeCave;
    public Vector2 spawnCenter;
    public float minSpawnVariance = .01f;
    public float maxSpawnVariance = 2f;
    private string currentGameState = "";
    private string lastGameState = "";
    public bool JoinComplete { get; private set; } = false;
    public delegate void OnUpdateClientTeam(int teamIndex, string clientID);
    public static event OnUpdateClientTeam onUpdateClientTeam;
    public GameObject hudContainer, gameOverPanel;
    public TextMeshProUGUI allianceTracker, foodCounter;
    public int winningTeam = -1;
    public float timeLimit = 60f;
    private float remainingTime;
    private bool _showCountDown = false;
    public bool gamePlaying { get; private set; }
    public Transform tdmSpawnCenter;
    public float tdmMinSpawnVariance = 200f;
    public float tdmMaxSpawnVariance = 500f;
    private int numTotalFood;
    [SerializeField] private List<StoneAgeTeam> teams = new List<StoneAgeTeam>();

    private void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        playerStats = prefab.GetComponent<CharController>(); 
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
        RoomController.onJoined += OnJoinedRoom;
        RoomController.onTeamUpdate += OnTeamUpdate;
        RoomController.onTeamReceive += OnFullTeamUpdate;

        onViewAdded += OnPlayerCreated;
    }

    private void OnDisable() 
    {
        RoomController.onAddNetworkEntity -= OnNetworkAdd;
        RoomController.onRemoveNetworkEntity -= OnNetworkRemove;
        RoomController.onJoined -= OnJoinedRoom;
        RoomController.onTeamUpdate -= OnTeamUpdate;
        RoomController.onTeamReceive -= OnFullTeamUpdate;

        onViewAdded -= OnPlayerCreated;
    }

    public Vector2 GetSpawnPoint(int teamIndex)
    {
        Vector3 pos = spawnCenter;

        if (teamIndex == 0)
        {
            pos.x += -spawnCenter.x * Random.Range(minSpawnVariance, maxSpawnVariance);
        }

        return pos;
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
            CreateView(entity);

        }
    }

    private void OnNetworkRemove(NetworkedEntity entity, StoneColyseusNetworkedEntityView view)
    {
        RemoveView(view);
    }
    
    private void CreateView(NetworkedEntity entity)
    {
        //LSLog.LogImportant("print: " + JsonUtility.ToJson(entity));
        StoneColyseusNetworkedEntityView newView = Instantiate(prefab);
        ColyseusManager.Instance.RegisterNetworkedEntityView(entity, newView);
        newView.gameObject.SetActive(true);

        LSLog.LogImportant($"Game Manager - New View Created!");

        onViewAdded?.Invoke(newView);
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

    public void SetCurrentUserAttributes(Dictionary<string, string> attributes)
    {
        ColyseusManager.NetSend("setAttribute",
            new AttributeUpdateMessage
            {
                userId = ColyseusManager.Instance.CurrentUser.sessionId,
                attributesToSet = attributes
            });
    }

    private void OnJoinedRoom(string customLogic)
    {
        ///IsCoop = string.Equals(customLogic, "starBossCoop");
        JoinComplete = true;
    }

    private void UpdateGameStates(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("currentGameState", out string currentServerGameState))
        {
            currentGameState = currentServerGameState;
        }

        if (attributes.TryGetValue("lastGameState", out string lastServerGameState))
        {
            lastGameState = lastServerGameState;
        }

        if (attributes.TryGetValue("winningTeamId", out string currentWinningTeam))
        {
            if(!int.TryParse(currentWinningTeam, out winningTeam))
            {
                LSLog.LogError($"Failed to parse currentWinningTeam: {currentWinningTeam}");
            }
        }
    }

    private void OnRoomStateChanged(MapSchema<string> attributes)
    {
        UpdateGameStates(attributes);
    }

    private void OnTeamUpdate(int teamIdx, string clientID, bool added)
    {
        StoneAgeTeam team = GetOrCreateTeam(teamIdx);

        if (added)
        {
            if (team.AddPlayer(clientID))
            {
                //Alert anyone that needs to know, clientID has been added to teamIdx
                onUpdateClientTeam?.Invoke(teamIdx, clientID);
            }
        }
        else
        {
            team.RemovePlayer(clientID);
        }
    }

    private void OnFullTeamUpdate(int teamIdx, string[] clients)
    {
        StoneAgeTeam team = GetOrCreateTeam(teamIdx);
        for (int i = 0; i < clients.Length; ++i)
        {
            if (team.AddPlayer(clients[i]))
            {
                //Alert anyone that needs to know, clientID has been added to teamIdx
                onUpdateClientTeam?.Invoke(teamIdx, clients[i]);
            }
        }
    }

    private StoneAgeTeam GetOrCreateTeam(int teamIdx)
    {
        StoneAgeTeam team = null;
        for (int i = 0; i < teams.Count; ++i)
        {
            if (teams[i].teamIndex.Equals(teamIdx))
            {
                team = teams[i];
            }
        }

        if (team == null)
        {
            //We have not created this team yet
            team = new StoneAgeTeam();
            team.teamIndex = teamIdx;
            teams.Add(team);
        }

        return team;
    }

    public int GetTeamIndex(string clientID)
    {
        for (int i = 0; i < teams.Count; ++i)
        {
            if (teams[i].ContainsClient(clientID))
            {
                return teams[i].teamIndex;
            }
        }

        LSLog.LogError($"Client {clientID} is not on a team!"); //We should not be asking for teams if we're not expecting to have them
        return -1;
    }

    public bool AreUsersSameTeam(CharControllerMulti clientA, CharControllerMulti clientB)
    {
        return clientA.TeamIndex.Equals(clientB.TeamIndex);
    }

    public CharControllerMulti GetPlayer()
    {
        NetworkedEntityView view = ColyseusManager.Instance.GetEntityView(ColyseusManager.Instance.CurrentNetworkedEntity.id);
        if (view != null)
        {
            CharControllerMulti pc = view as CharControllerMulti;
            if (pc)
            {
                return pc;
            }
        }

        LSLog.LogError($"Could not find a player for owner with ID {ColyseusManager.Instance.CurrentNetworkedEntity.id}");
        return null;
    }

    private void OnDestroy() 
    {
        ///ColyseusManager.Instance.OnEditorQuit();    
    }

    private void OnPlayerCreated(StoneColyseusNetworkedEntityView newView)
    {
        if (newView.TryGetComponent(out CharControllerMulti player))
        {
            if (!player.IsMine)
            {
                //player.InitializeObjectForRemote();
            }
        }
    }
}