using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : Scorable
{
    [SerializeField]
    private SpriteRenderer family;
    private IEnumerator agriculture;
    private string agricultureScore;

    private void OnDisable() {
        StopAllCoroutines();
    }

    public override void FinishObject(string ownerId)
    {
        base.FinishObject(ownerId);
        agriculture = Agriculture();
        SetTeamColor();
        if (ColyseusManager.Instance.CurrentUser.sessionId == ownerId) {
            StartCoroutine(agriculture);
            /*
            if (GameController.Instance.CurrentGameState == "SimulateRound") {

            }
            */
        }
    }

    IEnumerator Agriculture()
    {
        while (GameController.Instance.CurrentGameState == "SimulateRound") {
            GameController.Instance.RegisterSpend(ownerID, "AutoScore", "fruit", agricultureScore, ownerTeam.ToString(), "0");
            yield return new WaitForSeconds(3f);
        }

        yield break; 
    }

    private void SetTeamColor()
    {
        family.color = GameController.Instance.GetTeamColor(ownerTeam);
    }
}
