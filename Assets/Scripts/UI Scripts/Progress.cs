using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Progress : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI progress;

    [SerializeField]
    private TextMeshProUGUI cost;

    [SerializeField]
    private RectTransform rectTransform;
    private int team;
#pragma warning restore 0649
    private bool rival = false;

    void Awake()
    {
        SetColors(team);
    }

    public void SetProgress(int teamIndex, int type, string scorableProg)
    {
        this.gameObject.GetComponent<Image>().color = GameController.Instance.GetTeamColor(teamIndex);
        cost.text = "/" + scorableProg; 
    }

    public void UpdateProgress(int type, int progress)
    {
        
    }

    public void SetColors(int teamIdx)
    {
        //playerTag.color = GameController.Instance.GetTeamColor(teamIdx);    
    }
}
