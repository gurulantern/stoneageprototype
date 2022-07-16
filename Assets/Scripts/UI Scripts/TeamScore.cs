using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class TeamScore : MonoBehaviour
{
    public Image color;
    public TextMeshProUGUI[] scores;
    public int team;
    // Start is called before the first frame update
    void Start()
    {
        team = -1;
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(int teamIdx)
    {
        color.color = GameController.Instance.GetTeamColor(teamIdx);
        team = teamIdx;
        color.gameObject.SetActive(true);
    }

    public void Deinitialize(int teamIdx)
    {
        color.gameObject.SetActive(false);
        color.color = Color.white;
        team = -1;
    }

    public void Toggle(bool open)
    {
        this.GetComponent<Image>().enabled = open;
        foreach (TextMeshProUGUI score in scores) {
            score.gameObject.SetActive(open);
        }
    }
}


