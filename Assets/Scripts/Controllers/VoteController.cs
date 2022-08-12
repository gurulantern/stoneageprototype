using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LucidSightTools;
using System.Linq;

public class VoteController : MonoBehaviour
{
    private static VoteController instance;
    [SerializeField]
    private GameObject BG;
    public static VoteController Instance
    {
        get
        {
            if (instance == null)
            {
                LSLog.LogError("No VoteController in scene!");
            }

            return instance;
        }
    }
    [SerializeField] 
    private Transform camTransform;

    [SerializeField]
    private List<GameObject> wallsToVote;
    [SerializeField]
    private List<GameObject> voteButtons;
    [SerializeField]
    private GameObject prefab;

    private int currentPainting = 0;

    void Start()
    {
        instance = this;
    }

    public Transform GetVotePosition() => camTransform;

    public void ActivateVoteBG()
    {
        BG.gameObject.SetActive(true);
        BG.transform.position = new Vector3(camTransform.position.x, camTransform.position.y, 10f);
    }

    public void DeactivateVoteBG()
    {
        BG.gameObject.SetActive(false);
    }

    public void ReadyVoteButtons(GameObject wall)
    {
        TeamWall thisWall = wall.GetComponent<TeamWall>();
        wallsToVote.Add(wall);
        GameObject button = voteButtons[thisWall.ownerTeam];
        if (wallsToVote.Count > 1 && voteButtons.Count > 1) {
            wall.SetActive(false);
            button.SetActive(false);
        } else {
            wall.SetActive(true);
            button.SetActive(true);
        }
    }

    private void GetWallAndButton(int current, bool show)
    {
        GameObject wall = wallsToVote[current];
        wall.SetActive(show);
        voteButtons[wall.GetComponent<TeamWall>().ownerTeam].SetActive(show);
    }
    
    public void ShowNextPainting()
    {
        GetWallAndButton(currentPainting, false);
        currentPainting++;
        if (currentPainting > wallsToVote.Count - 1) {
            currentPainting = 0;
        }
        GetWallAndButton(currentPainting, true);
    }
}
