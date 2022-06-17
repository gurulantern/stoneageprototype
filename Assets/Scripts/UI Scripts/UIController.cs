using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private Button resetButton = null;
    [SerializeField]
    private Button startButton = null;
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

    private Dictionary<CharControllerMulti, PlayerTag> playerTags;

#pragma warning restore 0649
    public UnityEvent onStart;
    public UnityEvent onReset;

    public void UpdateGameStart(bool showButton)
    {
        startButton.gameObject.SetActive(showButton);
    }

    public void AllowReset(bool allowed)
    {
        resetButton.gameObject.SetActive(allowed);
    }

    public void ButtonOnStart()
    {
        onStart?.Invoke();
    }

    public void ButtonSetOptions()
    {

    }

    public void ButtonOnReset()
    {
        onReset?.Invoke();
    }

    public virtual void Update()
    {
        pingLabel.text = $"Ping: {ColyseusManager.Instance.GetRoundtripTime}ms";
    }

    public void UpdateGeneralMessageText(string message)
    {
        generalMessageText.text = message;
    }

    
    public void UpdateCountDownMessage(string message)
    {
        countDownText.text = message;
    }
}
