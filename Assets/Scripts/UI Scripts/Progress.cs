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
    private TextMeshProUGUI progCost;
    private Color teamColor;

    private int team;
#pragma warning restore 0649
    private bool rival = false;

    void Awake()
    {
        SetColors(team);
    }

    public void SetProgress(int teamIndex, string cost)
    {
        teamColor = GameController.Instance.GetTeamColor(teamIndex);
        this.gameObject.GetComponent<Image>().color = new Color(teamColor.r, teamColor.g, teamColor.b, .5f);
        progCost.text = "/" + cost; 
    }

    public void UpdateProgress(int prog)
    {
        progress.text = prog.ToString();            
    }

    public void SetColors(int teamIdx)
    {
        //playerTag.color = GameController.Instance.GetTeamColor(teamIdx);    
    }
}
