using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] private GameObject teamScore;
    public GameObject _headerParent;
    public List<GameObject> _headers;
    public List<TeamScore> scoreList;
    public List<TeamScore> activeScoreList;
    public TextMeshProUGUI _openButton;
    public TextMeshProUGUI _closeButton;
    private bool open;
    // Start is called before the first frame update
    void Start()
    {
        open = true;

    }

    public void UpdateScores(int teamIdx)
    {

    }

    /// Reorganizes the team scoreboards using the active score list while acounting for the extra child that is the header
    public void AddTeamScore(int teamIdx)
    {
        TeamScore addedScores = scoreList[teamIdx];
        addedScores.gameObject.GetComponent<RectTransform>().SetSiblingIndex(activeScoreList.Count - 1);
        activeScoreList.Add(addedScores);
        addedScores.Initialize(teamIdx);
    }

    public void RemoveTeamScore(int teamIdx)
    {
        TeamScore removedScores = scoreList[teamIdx];
        activeScoreList.Remove(removedScores);
        removedScores.Deinitialize(teamIdx);
        foreach (TextMeshProUGUI score in removedScores.scores) {
            score.gameObject.SetActive(false);
        }
        removedScores.gameObject.GetComponent<RectTransform>().SetAsLastSibling();      
    }

    public void ToggleScores()
    {
        open = !open;
        _headerParent.gameObject.SetActive(open);
        foreach(TeamScore score in activeScoreList)
        {
            score.Toggle(open);
        }

        if(open == true) {
            _openButton.gameObject.SetActive(false);
            _closeButton.gameObject.SetActive(true);
        } else {
            _closeButton.gameObject.SetActive(false);
            _openButton.gameObject.SetActive(true);
        }

    }

    public void UpdateShowScore(int scoreType, bool show)
    {
        _headers[scoreType].gameObject.SetActive(show);
        foreach(TeamScore score in activeScoreList)
        {
            score.scores[scoreType].gameObject.SetActive(show);
        }
    }

}

