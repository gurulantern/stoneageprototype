using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOptions : UIController
{
    private void CloseOptions()
    {
        gameOptions.gameObject.SetActive(false);
    }

    private void StartButton()
    {

    }

    private void OptionsButton()
    {
        gameOptions.gameObject.SetActive(true);
    }
}
