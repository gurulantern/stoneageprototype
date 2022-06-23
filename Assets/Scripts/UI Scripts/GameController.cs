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
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public StoneColyseusNetworkedEntityView prefab;
    public delegate void OnViewAdded(StoneColyseusNetworkedEntityView view);
    public static event OnViewAdded onViewAdded;

    public delegate void OnViewRemoved(StoneColyseusNetworkedEntityView view);
    public static event OnViewRemoved onViewRemoved;
    public UIController uiController;
    public static GameController Instance { get; private set; }
    CharController playerStats;
    [SerializeField] private Cave homeCave;
    public Vector2 spawnCenter;
    private TextMeshProUGUI scoreBoard;
    public float minSpawnVariance = .01f;
    public float maxSpawnVariance = 2f;
    private string currentGameState = "";
    private string lastGameState = "";
    public bool JoinComplete { get; private set; } = false;
    public bool IsCoop { get; private set; }
    public delegate void OnUpdateClientTeam(int teamIndex, string clientID);
    public static event OnUpdateClientTeam onUpdateClientTeam;
    public int winningTeam = -1;
    public float roundTimeLimit = 240f;
    public float paintTimeLimit = 60f;
    private float remainingTime;
    private bool _showCountDown = false;
    public bool gamePlaying { get; private set; } = false;
    public Transform SpawnCenter;
    public float MinSpawnVariance = 200f;
    public float MaxSpawnVariance = 500f;
    private int numTotalFood;
    [SerializeField] 
    private List<StoneAgeTeam> teams = new List<StoneAgeTeam>();

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
        gamePlaying = false;
    }

    private void OnEnable() 
    {
        
        RoomController.onRoomStateChanged += OnRoomStateChanged;
        RoomController.onBeginRoundCountDown += OnBeginRoundCountDown;
        RoomController.onBeginRound += OnBeginRound;
        RoomController.onRoundEnd += OnRoundEnd;
        RoomController.onJoined += OnJoinedRoom;
        RoomController.onAddNetworkEntity += OnNetworkAdd;
        RoomController.onRemoveNetworkEntity += OnNetworkRemove;
        RoomController.onJoined += OnJoinedRoom;
        RoomController.onTeamUpdate += OnTeamUpdate;
        RoomController.onTeamReceive += OnFullTeamUpdate;

        onViewAdded += OnPlayerCreated;

        uiController.UpdateCountDownMessage("");
        uiController.UpdateGeneralMessageText("");
    }

    private void OnDisable() 
    {
        RoomController.onRoomStateChanged -= OnRoomStateChanged;
        RoomController.onBeginRoundCountDown -= OnBeginRoundCountDown;
        RoomController.onBeginRound -= OnBeginRound;
        RoomController.onRoundEnd -= OnRoundEnd;
        RoomController.onJoined -= OnJoinedRoom;
        RoomController.onAddNetworkEntity -= OnNetworkAdd;
        RoomController.onRemoveNetworkEntity -= OnNetworkRemove;
        RoomController.onJoined -= OnJoinedRoom;
        RoomController.onTeamUpdate -= OnTeamUpdate;
        RoomController.onTeamReceive -= OnFullTeamUpdate;

        onViewAdded -= OnPlayerCreated;
    }

    /// <summary>
    /// Used with button input when the user wants to return to the lobby
    /// </summary>
    public void OnQuitGame()
    {
        if (ColyseusManager.Instance.IsInRoom)
        {
            //Find playerController for this player
            CharControllerMulti pc = GetPlayerView< CharControllerMulti>(ColyseusManager.Instance.CurrentNetworkedEntity.id);
            if (pc != null)
            {
                pc.enabled = false; //Stop all the messages and updates
            }

            ColyseusManager.Instance.LeaveAllRooms(() =>
            {
                ColyseusManager.Instance.ClearCollectionsAndUser();
                SceneManager.LoadScene(0);
            });
        }
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

    /// <summary>
    /// Used with button input when the user is ready to start a round of play
    /// </summary>
    public void PlayerReadyToPlay()
    {
        uiController.UpdatePlayerReadiness(false);

        SetCurrentUserAttributes(new Dictionary<string, string> { { "readyState", "ready" } });
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
            uiController.timer.DecrementTime(remainingTime / (roundTimeLimit * 100));
            Debug.Log(remainingTime);
        } 
    }

    public Vector2 GetSpawnPoint(int teamIndex)
    {
        Debug.Log("Getting spawn point.");
        Vector2 pos = SpawnCenter.position;

        if (teamIndex == 0)
        {
            //pos += (-SpawnCenter.right * Random.Range(MinSpawnVariance, MaxSpawnVariance));
        }
        else if (teamIndex == 1)
        {
            //pos += (SpawnCenter.right * Random.Range(MinSpawnVariance, MaxSpawnVariance));
        }

        Debug.Log("Got your spawn point!");
        return pos;
    }

    public void BeginGame()
    {
        gamePlaying = true;
        Time.timeScale = 1.0f;
    }

    private void EndGame()
    {
        gamePlaying = false;
        uiController.ShowGameOverScreen();
        Time.timeScale = 0f;
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
        IsCoop = string.Equals(customLogic, "collaborative");
        JoinComplete = true;
    }

       private void OnBeginRoundCountDown()
    {
        LSLog.LogImportant($"Round Count Down Has Begun!", LSLog.LogColor.cyan);

        _showCountDown = true;
    }

    private void OnBeginRound()
    {
        StartCoroutine(BeginRoutine());
    }

    private IEnumerator BeginRoutine()
    {
        if (IsCoop == false)
        {
            Debug.Log("Game has started!");
            CharControllerMulti pc = GetPlayer();

            if (pc)
            {
                pc.PositionAtSpawn();
            }
        }

        gamePlaying = true;
        if (IsCoop)
        {

        }
        else
        {
            winningTeam = -1;
        }

        yield return new WaitForSeconds(1.0f);

        _showCountDown = false;
        uiController.UpdateCountDownMessage("");
    }

    private void OnRoundEnd()
    {
        LSLog.LogImportant($"Round Ended!", LSLog.LogColor.lime);
        ///StartCoroutine(RoundEndRoutine());
    }
/*
    private IEnumerator RoundEndRoutine()
    {
        RoundInProgress = false;
        PlayerSpaceshipController ownersShip = GetOwnersShip();
        if (IsCoop)
        {

            uiController.ShowRoundOverMessage("VICTORY! THE WORM IS DEFEATED!");

        }
        else
        {
            //We may not have the winning team yet, need to hold here
            StartCoroutine(HoldForWinner());
        }

        ResetAllShipDamage();
    }
*/

    private void OnRoomStateChanged(MapSchema<string> attributes)
    {
        UpdateGameStates(attributes);
        UpdateGeneralMessage(attributes);
        UpdateCountDown(attributes);
        UpdateRoundTime(attributes);
    }

    private void UpdateGameStates(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("currentGameState", out string currentServerGameState))
        {
            currentGameState = currentServerGameState;
            Debug.Log(currentGameState + " / " + currentServerGameState);
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

    private void UpdateCountDown(MapSchema<string> attributes)
    {
        if (!_showCountDown)
        {
            return;
        }

        if(attributes.TryGetValue("countDown", out string countDown))
        {
            uiController.UpdateCountDownMessage(countDown);
        }
    }

    private void UpdateGeneralMessage(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("generalMessage", out string generalMessage))
        {
            uiController.UpdateGeneralMessageText(generalMessage);
        }
    }

    private void UpdateRoundTime(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("roundTime", out string time))
        {
            if (float.TryParse(time, out float serverTime))
            {
                remainingTime = serverTime;
            }
        }
    }

    private void OnTeamUpdate(int teamIdx, string clientID, bool added)
    {
        StoneAgeTeam team = GetOrCreateTeam(teamIdx);
        scoreBoard = uiController.scoreBoards[teamIdx];

        if (added)
        {
            if (team.AddPlayer(clientID))
            {
                //Alert anyone that needs to know, clientID has been added to teamIdx
                onUpdateClientTeam?.Invoke(teamIdx, clientID);
            }
            if (team.clientsOnTeam.Count == 1)
            {
                scoreBoard.gameObject.SetActive(true);
            }
        }
        else
        {
            team.RemovePlayer(clientID);
            if(team.clientsOnTeam.Count == 0)
            {
                scoreBoard.gameObject.SetActive(false);
            }
        }
    }

    private void OnFullTeamUpdate(int teamIdx, string[] clients)
    {
        StoneAgeTeam team = GetOrCreateTeam(teamIdx);
        scoreBoard = uiController.scoreBoards[teamIdx];

        for (int i = 0; i < clients.Length; ++i)
        {
            if (team.AddPlayer(clients[i]))
            {
                //Alert anyone that needs to know, clientID has been added to teamIdx
                onUpdateClientTeam?.Invoke(teamIdx, clients[i]);
            }
            if (team.clientsOnTeam.Count == 1)
            {
                scoreBoard.gameObject.SetActive(true);
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
            Debug.Log("Getting player");
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
    public T GetPlayerView<T>(string entityID) where T : StoneColyseusNetworkedEntityView
    {
        if (ColyseusManager.Instance.HasEntityView(entityID))
        {
            return ColyseusManager.Instance.GetEntityView(entityID) as T;
        }

        LSLog.LogError($"No player controller with id {entityID} found!");
        return null;
    }
}