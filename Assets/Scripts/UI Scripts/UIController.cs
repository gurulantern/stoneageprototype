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
    private Button optionsButton = null;
    [SerializeField]
    private Button setOptionsButton = null;
    [SerializeField]
    private Button closeOptionsButton = null;
    [SerializeField]
    private GameObject gameOptions = null;
    [SerializeField]
    private TextMeshProUGUI generalMessageText;
    [SerializeField]
    private TextMeshProUGUI countDownText;

    [SerializeField]
    private TextMeshProUGUI roundOverMessageText;

    //[SerializeField]
    //private PlayerInfoView playerInfo;

    [SerializeField]
    private CanvasGroup loadingCover;

    [SerializeField]
    private PlayerTag playerTagPrefab;

    [SerializeField]
    private RectTransform playerTagRoot;

    [SerializeField]
    private TextMeshProUGUI playerJoinMsgPrefab;

    [SerializeField]
    private RectTransform playerJoinMsgRoot;

    [SerializeField]
    private TextMeshProUGUI pingLabel;
    public Camera cam;

    private Dictionary<CharControllerMulti, PlayerTag> playerTags;
    public bool IsReady { get; private set; } = false;
    private Queue<GameObject> playerJoinMessages;

    private float currentMsgUpdate = 0;

    private CharControllerMulti myMetee;
    public GameObject hudContainer, gameOverPanel;
    public TextMeshProUGUI allianceTracker, foodCounter;



#pragma warning restore 0649
    public UnityEvent onPlayerReady;
    public UnityEvent onExit;
    public UnityEvent onReset;

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

            //playerInfo.gameObject.SetActive(false);

            while (GameController.Instance.JoinComplete == false)
            {
                yield return new WaitForEndOfFrame();
            }

            ///playerInfo.SetData(GameController.Instance.IsCoop == false);

            float t = 0.0f;
            while (t < 1.0f)
            {
                loadingCover.alpha = Mathf.Lerp(1, 0, t);
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }
            loadingCover.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        CharControllerMulti.onPlayerActivated += OnPlayerActivated;
        CharControllerMulti.onPlayerDeactivated += OnPlayerDeactivated;
        RoomController.onPlayerJoined += OnPlayerJoined;
        // For player list
        RoomController.onAddNetworkEntity += OnAddNetworkEntity;
        //RoomController.onRemoveNetworkEntity += OnRemoveNetworkEntity;
    }

    private void OnDisable()
    {
        CharControllerMulti.onPlayerActivated -= OnPlayerActivated;
        CharControllerMulti.onPlayerDeactivated -= OnPlayerDeactivated;
        RoomController.onPlayerJoined -= OnPlayerJoined;
        // For player list
        RoomController.onAddNetworkEntity -= OnAddNetworkEntity;
        //RoomController.onRemoveNetworkEntity -= OnRemoveNetworkEntity;
    }

        private void OnPlayerActivated(CharControllerMulti meteeController)
    {
        if (!IsReady)
        {
            StartCoroutine(Init());
        }

        if (meteeController.IsMine)
        {
            myMetee = meteeController;
            return;
        }

        if (playerTags.ContainsKey(meteeController) == false)
        {
            PlayerTag newPlayerTag = Instantiate(playerTagPrefab);

            newPlayerTag.transform.SetParent(playerTagRoot);
            newPlayerTag.SetPlayerTag(string.IsNullOrEmpty(meteeController.UserName) ? meteeController.Id : meteeController.UserName, meteeController.TeamIndex);

            playerTags.Add(meteeController, newPlayerTag);
        }
    }

    private void OnPlayerDeactivated(CharControllerMulti meteeController)
    {
        if (meteeController.IsMine) return;

        if (playerTags.ContainsKey(meteeController))
        {
            PlayerTag playerTag = playerTags[meteeController];

            Destroy(playerTag.gameObject);

            playerTags.Remove(meteeController);
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

    }

    public void ButtonOnReset()
    {
        onReset?.Invoke();
    }
    //Open function for the Options button
    public void OpenOptions()
    {
        gameOptions.gameObject.SetActive(true);
    }

    //Close function for the Options menus
    public void CloseOptions()
    {
        gameOptions.gameObject.SetActive(false);
    }
    /// Shows Game Over screen at the end of a round. Might convert for Paint round.
    public void ShowGameOverScreen()
    {
        hudContainer.SetActive(false);
        gameOverPanel.SetActive(true);
    }


    public virtual void Update()
    {
        pingLabel.text = $"Ping: {ColyseusManager.Instance.GetRoundtripTime}ms";
        UpdatePlayerJoinMessage();
    }

    public void LateUpdate()
    {
        UpdatePlayerTags();
    }

    private void UpdatePlayerTags()
    {
        foreach (KeyValuePair<CharControllerMulti, PlayerTag> pair in playerTags)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(playerTagRoot, RectTransformUtility.WorldToScreenPoint(cam, pair.Key.transform.position), null, out Vector2 pos))
            {
                pair.Value.UpdateTag(pos, 1, myMetee.TeamIndex);
            }
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
/*
    private void OnRemoveNetworkEntity(NetworkedEntity entity, StoneColyseusNetworkedEntityView view)
    {
        playerInfo.RemovePlayer(entity);
    }
*/
}
