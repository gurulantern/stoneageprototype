using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownControl : MonoBehaviour
{
    // Start is called before the first frame update
   private int countdownTime;
   public TextMeshProUGUI countdownDisplay;
   public Button startButton;

    void Start()
    {
        countdownTime = 5;
    }
   public void StartGame()
    {
        startButton.gameObject.SetActive(false);
        StartCoroutine(CountdownToStart());
    }

   IEnumerator CountdownToStart()
   {
       while(countdownTime > 0)
       {
           countdownDisplay.text = countdownTime.ToString();
           yield return new WaitForSeconds(1f);

           countdownTime--;
       }

       countdownDisplay.text = "Start!";

       yield return new WaitForSeconds(1f);

       GameController.instance.BeginGame();

       countdownDisplay.gameObject.SetActive(false);
   }
}
