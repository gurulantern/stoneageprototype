using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoteButton : MonoBehaviour
{
    [SerializeField]
    private int team;
    [SerializeField]
    private RectTransform rectTransform;

    public delegate void OnVote();
    public static event OnVote onVote;

    public void Vote()
    {
        GameController.Instance.RegisterVote(team);
        onVote?.Invoke();
    }
}
