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
    private CanvasGroup canvasGroup;

    [SerializeField]
    private RectTransform rectTransform;
    private int team;
    public Color[] teamColors;
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

    public void UpdateTag(Vector2 position, float alpha, int teamIdx)
    {
        rectTransform.anchoredPosition = position;
        canvasGroup.alpha = alpha;

        SetColors(team);
    }

    private void SetColors(int teamIdx)
    {
        playerTag.color = teamColors[teamIdx];
    }
}

