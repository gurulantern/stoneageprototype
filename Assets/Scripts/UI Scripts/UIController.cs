using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private Button resetButton = null;

    [SerializeField]
    private Button startButton = null;

    [SerializeField]
    private TextMeshProUGUI pingLabel;
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

    public void ButtonOnReset()
    {
        onReset?.Invoke();
    }

    public virtual void Update()
    {
        pingLabel.text = $"Ping: {ColyseusManager.Instance.GetRoundtripTime}ms";
    }
}
