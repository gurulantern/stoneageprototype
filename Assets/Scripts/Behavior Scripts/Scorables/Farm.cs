using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : Scorable
{
    [SerializeField]
    private SpriteRenderer family;
    private IEnumerator agriculture;
    [SerializeField]
    private string agricultureScore;

    private void OnDisable() {
        StopAllCoroutines();
    }

    public override void FinishObject(string ownerId, string entityId)
    {
        base.FinishObject(ownerId, entityId);
        SetTeamColor();
        agriculture = Agriculture();
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
            GameController.Instance.RegisterSpend(entityID, "AutoScore", "fruit", agricultureScore, ownerTeam.ToString(), "0");
            yield return new WaitForSeconds(4f);
        }

        yield break; 
    }

    private void SetTeamColor()
    {
        family.color = GameController.Instance.GetTeamColor(ownerTeam);
    }
}
