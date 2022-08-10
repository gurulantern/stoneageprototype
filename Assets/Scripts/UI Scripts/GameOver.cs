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
        endText.gameObject.SetActive(!showText);
        finalScores.SetActive(!showScores);
    }

    public void UpdateText(int winner) 
    {
        if (winner > -1) {
            nobodyWins.SetActive(false);
            teams[winner].gameObject.SetActive(true);
        }
    }
}
