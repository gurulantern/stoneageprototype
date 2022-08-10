using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

 
public class UIController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private Button resetButton = null;
    [SerializeField]
    private Button readyButton = null;
    [SerializeField]
    private Button exitButton = null;
    [SerializeField]
    private Button optionsButton = null;
    [SerializeField]
    private Button setOptionsButton = null;
    [SerializeField]
    private Button closeOptionsButton = null;
    [SerializeField]
    private Canvas _canvas = null;
    public GameObject gameOptions;
    [SerializeField]
    private TextMeshProUGUI generalMessageText;
    [SerializeField]
    private TextMeshProUGUI countDownText;

    [SerializeField]
    private TextMeshProUGUI roundOverMessageText;
    [SerializeField]
    private CreateMenu _createMenu;

    //[SerializeField]
    //private PlayerInfoView playerInfo;


    public GameObject loadCover;

    [SerializeField]
    private PlayerTag playerTagPrefab;

    public RectTransform playerTagRoot;

    //[SerializeField]
    //private ProgressContainer progressPrefab;
    //public RectTransform progressRoot;

    [SerializeField]
    private TextMeshProUGUI playerJoinMsgPrefab;
    [SerializeField]
    private RectTransform playerJoinMsgRoot;

    [SerializeField]
    private TextMeshProUGUI pingLabel;

    public ObserveMeter _observeMeter;
    public Camera cam;
    public Scoreboard scoreboard;
    public Scoreboard finalScoreboard;



    private Dictionary<CharControllerMulti, PlayerTag> playerTags;
    //private Dictionary<Scorable, ProgressContainer> progressCounters;
    public bool IsReady { get; private set; } = false;
    public bool HideTags { get; set; } = false;
    private Queue<GameObject> playerJoinMessages;

    private float currentMsgUpdate = 0;

    private CharControllerMulti player;
    public Timer timer;
    public GameObject hudContainer;
    [SerializeField]
    private GameOver gameOverPanel;
    public TextMeshProUGUI allianceTracker;
    public GameObject woodCount, seedsCount, scareControl, createControl;



#pragma warning restore 0649
    public UnityEvent onPlayerReady;
    public UnityEvent onExit;
    public UnityEvent onSetOptions;

    private IEnumerator Start()
    {
        yield return StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        if (!IsReady)
        {

            IsReady = true;

            ///roundOverMessageText.gameObject.SetActive(false);
            playerTags = new Dictionary<CharControllerMulti, PlayerTag>();
            playerJoinMessages = new Queue<GameObject>();

            //progressCounters = new Dictionary<Scorable, ProgressContainer>();

            //playerInfo.gameObject.SetActive(false);

            while (GameController.Instance.JoinComplete == false)
            {
                yield return new WaitForEndOfFrame();
            }

            ///playerInfo.SetData(GameController.Instance.IsCoop == false);


            if (GameController.Instance.gamePlaying)
            {
                readyButton.gameObject.SetActive(false);
                optionsButton.gameObject.SetActive(false);
                exitButton.gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        CharControllerMulti.onPlayerActivated += OnPlayerActivated;
        CharControllerMulti.onPlayerDeactivated += OnPlayerDeactivated;
        RoomController.onPlayerJoined += OnPlayerJoined;
        //GameController.onReset += ResetUI;

        // For player list
        RoomController.onAddNetworkEntity += OnAddNetworkEntity;
        //RoomController.onRemoveNetworkEntity += OnRemoveNetworkEntity;
    }

    private void OnDisable()
    {
        CharControllerMulti.onPlayerActivated -= OnPlayerActivated;
        CharControllerMulti.onPlayerDeactivated -= OnPlayerDeactivated;
        RoomController.onPlayerJoined -= OnPlayerJoined;
        //GameController.onReset -= ResetUI;
        // For player list
        RoomController.onAddNetworkEntity -= OnAddNetworkEntity;
        //RoomController.onRemoveNetworkEntity -= OnRemoveNetworkEntity;
    }

    private void ResetUI()
    {
        //timer.ResetTimer();
    }

    private void OnPlayerActivated(CharControllerMulti playerController)
    {
        if (!IsReady)
        {
            StartCoroutine(Init());
        }

        if (playerController.IsMine)
        {
            player = playerController;
            return;
        }

        if (playerTags.ContainsKey(playerController) == false && HideTags == false)
        {
            PlayerTag newPlayerTag = Instantiate(playerTagPrefab);

            newPlayerTag.transform.SetParent(playerTagRoot);
            newPlayerTag.SetPlayerTag(string.IsNullOrEmpty(playerController.UserName) ? playerController.Id : playerController.UserName, playerController.TeamIndex);

            playerTags.Add(playerController, newPlayerTag);
        }

        
    }

    private void OnPlayerDeactivated(CharControllerMulti player)
    {
        if (player.IsMine) return;

        if (playerTags.ContainsKey(player))
        {
            PlayerTag playerTag = playerTags[player];

            Destroy(playerTag.gameObject);

            playerTags.Remove(player);
        }
    }

    private void OnPlayerJoined(string playerUserName)
    {
        if (!IsReady)
        {
            StartCoroutine(Init());
        }

        if (string.IsNullOrEmpty(playerUserName) == false)
        {
            TextMeshProUGUI msg = Instantiate(playerJoinMsgPrefab);

            msg.transform.SetParent(playerJoinMsgRoot);

            msg.text = $"Player Joined: {playerUserName}";

            playerJoinMessages.Enqueue(msg.gameObject);
        }
    }


    public void UpdatePlayerReadiness(bool showButton)
    {
        readyButton.gameObject.SetActive(showButton);
        optionsButton.gameObject.SetActive(showButton);
        exitButton.gameObject.SetActive(showButton);
        countDownText.gameObject.SetActive(!showButton);
    }

    public void AllowExit(bool allowed)
    {
        //exitButton.gameObject.SetActive(allowed);
    }

    public void AllowReset(bool allowed)
    {
        resetButton.gameObject.SetActive(allowed);
    }
    public void ButtonOnReady()
    {
        onPlayerReady?.Invoke();
    }
    
    public void ButtonOnExit()
    {
        onExit?.Invoke();
    }

    public void ButtonSetOptions()
    {
        onSetOptions?.Invoke();
    }

    //Open function for the Options button
    public void OpenOptions()
    {
        readyButton.gameObject.SetActive(false);
        optionsButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        gameOptions.gameObject.SetActive(true);
    }

    //Close function for the Options menus
    public void CloseOptions()
    {
        gameOptions.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);
    }
    /// Shows Game Over screen at the end of a round. Might convert for Paint round.
    public void ShowGameOverScreen(int winningTeam)
    {
        gameOverPanel.UpdateText(winningTeam);
        hudContainer.SetActive(false);
        gameOverPanel.gameObject.SetActive(true);
    }

    public void HideGameOverScreen()
    {
        gameOverPanel.gameObject.SetActive(false);
        hudContainer.SetActive(true);
    }


    public virtual void Update()
    {
        pingLabel.text = $"Ping: {ColyseusManager.Instance.GetRoundtripTime}ms";
        UpdatePlayerJoinMessage();

        if (HideTags == false)
        {
            UpdatePlayerTags();
        }
        //UpdateProgressCounters();
    }

    private void UpdatePlayerTags()
    {
        foreach (KeyValuePair<CharControllerMulti, PlayerTag> pair in playerTags)
        {
            pair.Value.UpdateTag(Camera.main.WorldToScreenPoint(pair.Key.transform.position), pair.Key.TeamIndex);
        }
    }

    private void UpdatePlayerJoinMessage()
    {
        if (playerJoinMessages.Count > 0)
        {
            currentMsgUpdate += Time.unscaledDeltaTime;

            // remove a join message every 5 seconds
            if (currentMsgUpdate >= 5)
            {
                GameObject msg = playerJoinMessages.Dequeue();

                Destroy(msg);

                currentMsgUpdate = 0;
            }

        }
        else
        {
            currentMsgUpdate = 0;
        }
    }

    public void UpdateGeneralMessageText(string message)
    {
        generalMessageText.text = message;
    }

    
    public void UpdateCountDownMessage(string message)
    {
        countDownText.text = message;
    }

    private void OnAddNetworkEntity(NetworkedEntity entity)
    {
        StartCoroutine(WaitAddEntity(entity));
    }

    private IEnumerator WaitAddEntity(NetworkedEntity entity)
    {
        while (!GameController.Instance.JoinComplete)
        {
            yield return new WaitForEndOfFrame();
        }
        //playerInfo.AddPlayer(entity);
    }



}

