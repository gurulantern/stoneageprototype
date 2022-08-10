using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class TeamScore : MonoBehaviour
{
    public Image color;
    public GameObject _scoreParent;
    public TextMeshProUGUI[] scores;
    public int team;
    [SerializeField]
    private bool final;
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
        if (final == true) {
            _scoreParent.gameObject.SetActive(true);
        }
    }

    public void Deinitialize(int teamIdx)
    {
        color.gameObject.SetActive(false);
        color.color = Color.white;
        team = -1;
    }

    public void Toggle(bool open)
    {
        _scoreParent.gameObject.SetActive(open);
    }
}


