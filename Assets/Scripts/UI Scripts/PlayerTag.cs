using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerTag : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private TextMeshProUGUI playerTag;

    [SerializeField]
    private RectTransform rectTransform;
    private int team;
#pragma warning restore 0649
    private bool rival = false;

    void Awake()
    {
        SetColors(team);
    }

    public void SetPlayerTag(string tag, int teamIndex)
    {
        playerTag.text = tag;
        team = teamIndex;
    }

    public void UpdateTag(Vector2 position, int teamIdx)
    {
        rectTransform.position = position;
        //canvasGroup.alpha = alpha;

        SetColors(teamIdx);
    }

    public void HideTag() => this.gameObject.SetActive(false);

    private void SetColors(int teamIdx)
    {
        playerTag.color = GameController.Instance.GetTeamColor(teamIdx);    
    }
}

