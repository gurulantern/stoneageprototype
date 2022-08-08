using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AurochsPen : Scorable
{
    private IEnumerator domestication;
    [SerializeField]
    private string domesticateScore;

    private void OnDisable() {
        StopAllCoroutines();
    }
    public virtual void CaughtAurochs(Aurochs aurochs)
    {
        states[1].SetActive(false);
        states[2].SetActive(true);
        aurochs.Domesticate();
        domestication = Domestication();
        StartCoroutine(domestication);
    }

    IEnumerator Domestication()
    {
        while (GameController.Instance.CurrentGameState == "SimulateRound") {
            GameController.Instance.RegisterSpend(entityID, "AutoScore", "meat", domesticateScore, ownerTeam.ToString(), "0");
            yield return new WaitForSeconds(8f);
        }

        yield break; 
    }

}
