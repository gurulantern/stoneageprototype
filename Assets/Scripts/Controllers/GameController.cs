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
using UnityEngine.AI;
using System;

public class GameController : MonoBehaviour
{
    public StoneColyseusNetworkedEntityView prefab;
    public bool create, steal, scare, alliances;
    public delegate void OnViewAdded(StoneColyseusNetworkedEntityView view, NetworkedEntity entity);
    public static event OnViewAdded onViewAdded;
    public delegate void OnViewRemoved(StoneColyseusNetworkedEntityView view);
    public static event OnViewRemoved onViewRemoved;
    public UIController _uiController;
    public EnvironmentController  _environmentController;
    public static GameController Instance { get; private set; }
    private TextMeshProUGUI scoreBoard;
    private string currentGameState = "";
    private string lastGameState = "";
    public bool JoinComplete { get; private set; } = false;
    public bool IsCoop { get; private set; }
    public delegate void OnUpdateClientTeam(int teamIndex, string clientID);
    public static event OnUpdateClientTeam onUpdateClientTeam;
    public int winningTeam = -1;
    public float roundTimeLimit;
    public float paintTimeLimit = 60f;
    private float elapsedTime;
    private bool _showCountDown = false;
    public bool gamePlaying { get; private set; } = false;

    [SerializeField] 
    private List<StoneAgeTeam> teams = new List<StoneAgeTeam>();
    public  Cave[] homeCaves;
    public SpawnPoint[] aurochsSpawnPoints;
    public SpawnPoint[] deadAurochsSpawnPounts;
    [SerializeField] private Color[] teamColors;
    private  string[] scoreTypes = new string[] {"gather", "observe", "create", "total"};


    private void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        RoomController.onBeginPaint += OnBeginPaint;
        RoomController.onBeginVote += OnBeginVote;
        RoomController.onRoundEnd += OnRoundEnd;
        RoomController.onJoined += OnJoinedRoom;
        RoomController.onAddNetworkEntity += OnNetworkAdd;
        RoomController.onRemoveNetworkEntity += OnNetworkRemove;
        RoomController.onTeamUpdate += OnTeamUpdate;
        RoomController.onTeamReceive += OnFullTeamUpdate;

        onViewAdded += OnPlayerCreated;

        _uiController.UpdateCountDownMessage("");
        _uiController.UpdateGeneralMessageText("");
    }

    private void OnDisable() 
    {
        RoomController.onRoomStateChanged -= OnRoomStateChanged;
        RoomController.onBeginRoundCountDown -= OnBeginRoundCountDown;
        RoomController.onBeginRound -= OnBeginRound;
        RoomController.onBeginPaint -= OnBeginPaint;
        RoomController.onBeginVote -= OnBeginVote;
        RoomController.onRoundEnd -= OnRoundEnd;
        RoomController.onJoined -= OnJoinedRoom;
        RoomController.onAddNetworkEntity -= OnNetworkAdd;
        RoomController.onRemoveNetworkEntity -= OnNetworkRemove;
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
            CharControllerMulti pc = GetPlayerView<CharControllerMulti>(ColyseusManager.Instance.CurrentNetworkedEntity.id);
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
        _uiController.UpdatePlayerReadiness(false);

        SetCurrentUserAttributes(new Dictionary<string, string> { { "readyState", "ready" } });
    }

    public void SetSettings()
    {
        
    }
    
    private void CreateView(NetworkedEntity entity)
    {
        LSLog.LogImportant("print: " + JsonUtility.ToJson(entity));
        StoneColyseusNetworkedEntityView newView = Instantiate(prefab);
        ColyseusManager.Instance.RegisterNetworkedEntityView(entity, newView);
        newView.gameObject.SetActive(true);

        LSLog.LogImportant($"Game Manager - New View Created!");

        onViewAdded?.Invoke(newView, entity);
    }

    private void RemoveView(StoneColyseusNetworkedEntityView view)
    {
        view.SendMessage("OnEntityRemoved", SendMessageOptions.DontRequireReceiver);
    }
    private void FixedUpdate()
    {
        if (elapsedTime < roundTimeLimit && gamePlaying == true)
        {
            _uiController.timer.DecrementTime((roundTimeLimit - elapsedTime) / roundTimeLimit);
        }
        //Surface2D.UpdateNavMesh(Surface2D.navMeshData); 
    }

    public void BeginGame()
    {
        gamePlaying = true;
        Time.timeScale = 1.0f;
    }

    private void EndGame()
    {
        gamePlaying = false;
        _uiController.ShowGameOverScreen();
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

    private void OnBeginRound(float time)
    {
        StartCoroutine(BeginRoutine(time));
    }

    private IEnumerator BeginRoutine(float time)
    {
        /*
        if (IsCoop == false)
        {
            Debug.Log("Game has started!");
            CharControllerMulti pc = GetPlayer();

            if (pc)
            {
                pc.PositionAtSpawn();
            }
        }

        if (IsCoop)
        {

        }
        else
        {
            winningTeam = -1;
        }
        */
        roundTimeLimit = time;
        gamePlaying = true;
        Debug.Log("Game has started - round time: " + roundTimeLimit);

        yield return new WaitForSeconds(1.0f);

        _showCountDown = false;
        _uiController.UpdateCountDownMessage("");
    }

    private void OnBeginPaint(float time)
    {
        StartCoroutine(BeginPaint(time));
    }

    private IEnumerator BeginPaint(float time)
    {
        roundTimeLimit = time;
        Debug.Log("Paint has started, round time: " + roundTimeLimit);
        yield break;
    }

    private void OnBeginVote(float time)
    {
        StartCoroutine(BeginVote(time));
    }

    private IEnumerator BeginVote(float time)
    {
        roundTimeLimit = time;
        Debug.Log("Vote has started, round time: " + roundTimeLimit);
        yield break;
    }

    private void OnRoundEnd()
    {
        LSLog.LogImportant($"Round Ended!", LSLog.LogColor.lime);
        StartCoroutine(RoundEndRoutine());
    }

    private IEnumerator RoundEndRoutine()
    {
        gamePlaying = false;
        
            //We may not have the winning team yet, need to hold here
        //StartCoroutine(HoldForWinner());

        //ResetAllShipDamage();
        _uiController.UpdatePlayerReadiness(true);
        yield break;
    }


    private void OnRoomStateChanged(MapSchema<string> attributes)
    {
        UpdateGameStates(attributes);
        UpdateGeneralMessage(attributes);
        UpdateCountDown(attributes);
        if (gamePlaying)
        {
            UpdateRoundTime(attributes);
        }
    }

    public void UpdateSettings(Dictionary<string, string> options)
    {
        CharControllerMulti pc = GetPlayer();
        if (options.TryGetValue("observeReq", out string observeRequirement))
        {
            _uiController._observeMeter.observeSlider.maxValue = int.Parse(observeRequirement);
            //Debug.Log("Setting observe req");
        }

        if (options.TryGetValue("tireRate", out string newTireRate))
        {
            pc.tireRate = float.Parse(newTireRate);
        }

        if (options.TryGetValue("restRate", out string newRestRate))
        {
            pc.restoreRate = float.Parse(newRestRate);
        }

        if (options.TryGetValue("alliances", out string alianceBool))
        {
            alliances = bool.Parse(alianceBool);
        }

        if (options.TryGetValue("hideTags", out string tagsBool))
        {
            _uiController.playerTagRoot.gameObject.SetActive(!bool.Parse(tagsBool));
        }

        if (options.TryGetValue($"team{pc.TeamIndex}steal", out string stealBool))
        {
            steal = bool.Parse(stealBool);
        }

        if (options.TryGetValue($"team{pc.TeamIndex}scare", out string scareBool))
        {
            scare = bool.Parse(scareBool);
            _uiController.scareControl.SetActive(scare);
        }

        if (options.TryGetValue($"team{pc.TeamIndex}create", out string createBool))
        {
            create = bool.Parse(createBool);
            _uiController.scoreboard.UpdateShowScore(2, create);
            _uiController.woodCount.SetActive(create);
            _uiController.seedsCount.SetActive(create);
            _uiController.createControl.SetActive(create);
        }
    }

    private void UpdateGameStates(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("currentGameState", out string currentServerGameState))
        {
            currentGameState = currentServerGameState;
            //Debug.Log(currentGameState + " / " + currentServerGameState);
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
            _uiController.UpdateCountDownMessage(countDown);
        }
    }

    private void UpdateGeneralMessage(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("generalMessage", out string generalMessage))
        {
            _uiController.UpdateGeneralMessageText(generalMessage);
        }
    }

    private void UpdateRoundTime(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("elapsedTime", out string time))
        {
            if (float.TryParse(time, out float serverTime))
            {
                elapsedTime = serverTime/1000;
            }
        }
    }

    public void UpdateScores(string teamIndex, string scoreType, string updatedScore)
    {
        int scoreIndex = Array.IndexOf(scoreTypes, scoreType);

        _uiController.scoreboard.scoreList[int.Parse(teamIndex)].scores[scoreIndex].text = updatedScore;

        if (scoreType == "observe") {
            GameController.Instance._uiController._observeMeter.Increment(int.Parse(updatedScore));
        }   else if (scoreType == "create") {

        }
    }

    public void Unlock(string teamIdx, string observedObject)
    {

    }

    private void OnTeamUpdate(int teamIdx, string clientID, bool added)
    {
        StoneAgeTeam team = GetOrCreateTeam(teamIdx);
        //scoreBoard = _uiController.scoreBoards[teamIdx];

        if (added)
        {
            if (team.AddPlayer(clientID))
            {
                //Alert anyone that needs to know, clientID has been added to teamIdx
                onUpdateClientTeam?.Invoke(teamIdx, clientID);
            }
            if (team.clientsOnTeam.Count == 1)
            {
                _uiController.scoreboard.AddTeamScore(teamIdx);
                _uiController.gameOptions.GetComponent<GameOptions>().AddTeamOptions(teamIdx);
            }
        }
        else
        {
            team.RemovePlayer(clientID);
            if(team.clientsOnTeam.Count == 0)
            {
                _uiController.scoreboard.RemoveTeamScore(teamIdx);
                _uiController.gameOptions.GetComponent<GameOptions>().RemoveTeamOptions(teamIdx);

            }
        }
    }

    private void OnFullTeamUpdate(int teamIdx, string[] clients)
    {
        StoneAgeTeam team = GetOrCreateTeam(teamIdx);
        //scoreBoard = _uiController.scoreBoards[teamIdx];

        for (int i = 0; i < clients.Length; ++i)
        {
            if (team.AddPlayer(clients[i]))
            {
                //Alert anyone that needs to know, clientID has been added to teamIdx
                onUpdateClientTeam?.Invoke(teamIdx, clients[i]);
            }
            if (team.clientsOnTeam.Count == 1)
            {
                _uiController.scoreboard.AddTeamScore(teamIdx);
                _uiController.gameOptions.GetComponent<GameOptions>().AddTeamOptions(teamIdx);
            } else if (team.clientsOnTeam.Count == 0) {
                _uiController.scoreboard.RemoveTeamScore(teamIdx);
                _uiController.gameOptions.GetComponent<GameOptions>().RemoveTeamOptions(teamIdx);
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
        //Adds team color options to the menu

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

    public Color GetTeamColor(int teamIdx)
    {
        return teamColors[teamIdx];
    }

    public int GetTeamNumber(int teamIdx)
    {
        if(teams[teamIdx].clientsOnTeam.Count <= 10)
        {
            return teams[teamIdx].clientsOnTeam.Count - 1;
        } else {
            LSLog.LogError($"Team of {teamIdx} full");
            return -1;
        }
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

    private void OnPlayerCreated(StoneColyseusNetworkedEntityView newView, NetworkedEntity entity)
    {
        if (newView.TryGetComponent(out CharControllerMulti player))
        {
            if (!player.IsMine)
            {
                player.InitializeObjectForRemote(entity);
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

    public void RegisterGather(string entityID, string fruit, string meat, string teamIndex)
    {
        ColyseusManager.CustomServerMethod("gather", new object[] { entityID, fruit, meat, teamIndex});
    }

    public void RegisterObserve(string entityID, string observedObject, string teamIndex)
    {
        ColyseusManager.CustomServerMethod("observe", new object[] { entityID, observedObject, teamIndex});
    }

    public void RegisterCreate(string entityID, string createType, string teamIndex)
    {
        ColyseusManager.CustomServerMethod("create", new object[] { entityID, createType, teamIndex });
    }
}
