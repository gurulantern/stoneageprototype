using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] private GameObject teamScore;
    [SerializeField] private GameObject _headers;
    public UnityEvent onToggleScores;
    public List<TeamScore> scoreList;
    public List<TeamScore> activeScoreList;
    private bool open;
    // Start is called before the first frame update
    void Start()
    {
        open = false;
    }
    void OnEnable()
    {
        //TeamScore.onToggleScores += ToggleScores;
    }

    public void AddTeamScore(int teamIdx)
    {
        GameObject newTeamScore = scoreList[teamIdx].gameObject;
    }
/*
    public void AddTeamScore(int teamIdx)
    {
        GameObject newTeamScore = Instantiate(teamScore, this.gameObject.transform);
        newTeamScore.GetComponent<TeamScore>().Initialize(teamIdx);
        newTeamScore.name = $"Team {teamIdx} Scores";
    }

    public void RemoveTeamScore(int teamIdx)
    {
        scoreArr = GetComponentsInChildren<TeamScore>();
        foreach (TeamScore score in scoreArr)
        {
            if (score.team == teamIdx)
            {
                Destroy(score.gameObject);
            }
        }
    }

    public void ToggleScores()
    {
        _headers.SetActive(!open);
        scoreArr = GetComponentsInChildren<TeamScore>();
        foreach(TeamScore score in scoreArr)
        {
            score.Hide(!open);
        }
        open = !open;
    }
*/
}

