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

    [SerializeField]
    private Color teamColor;

    [SerializeField]
    private Color rivalColor;
#pragma warning restore 0649
    private bool rival = false;

    void Awake()
    {
        SetColors();
    }

    public void SetPlayerTag(string tag)
    {
        playerTag.text = tag;
    }

    public void UpdateTag(Vector2 position, float alpha, bool isFriendly)
    {
        rectTransform.anchoredPosition = position;
        canvasGroup.alpha = alpha;

        if (isFriendly != rival)
        {
            rival = isFriendly;
            SetColors();
        }
    }

    private void SetColors()
    {
        playerTag.color = rival ? teamColor : rivalColor;
    }
}

