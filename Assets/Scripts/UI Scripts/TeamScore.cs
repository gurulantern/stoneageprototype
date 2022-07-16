using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class TeamScore : MonoBehaviour
{
    [SerializeField] private Image color;
    [SerializeField] private TextMeshProUGUI foodScore;
    [SerializeField] private TextMeshProUGUI observeScore;
    [SerializeField] private TextMeshProUGUI createScore;
    [SerializeField] private TextMeshProUGUI totalScore;
    public int team;
    public UnityEvent onToggleScores;
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
        this.gameObject.SetActive(true);
    }

    public void Deinitialize(int teamIdx)
    {
        this.gameObject.SetActive(false);
        color.color = Color.white;
        team = -1;
    }

    public void Hide(bool open)
    {
        this.GetComponent<Image>().enabled = open;
        foodScore.gameObject.SetActive(open);
        observeScore.gameObject.SetActive(open);
        createScore.gameObject.SetActive(open);
        totalScore.gameObject.SetActive(open);
    }

    public void OnToggleButton()
    {
        onToggleScores?.Invoke();
    }
}


