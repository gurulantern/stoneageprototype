using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOver : MonoBehaviour
{
    [SerializeField]
    private GameObject finalScores;
    [SerializeField]
    private GameObject endText;
    [SerializeField]
    private GameObject nobodyWins;
    [SerializeField]
    private List<GameObject> teams; 
    private bool showScores = false;
    private bool showText = true;
    public void ShowScores() 
    {
        showText = !showText;
        showScores = !showScores;
        endText.gameObject.SetActive(showText);
        finalScores.SetActive(showScores);
    }

    public void UpdateText(int winner) 
    {
        nobodyWins.SetActive(false);
        teams[winner].gameObject.SetActive(true);
    }

    public void Reset()
    {
        GameController.Instance.RegisterReset();
    }
}
