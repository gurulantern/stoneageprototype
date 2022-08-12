using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamWall : MonoBehaviour
{
    [SerializeField]
    private Transform playerPos;
    
    public int ownerTeam;

    public void SetOwnerTeam(int team, Transform pos)
    {
        ownerTeam = team;
        playerPos = pos;
        this.gameObject.SetActive(false);

        //GameController.onReset += Destroy;
    }

    private void OnDisable() {
        //GameController.onReset -= Destroy;    
    }

    public void Reveal()
    {
        transform.position = new Vector3(playerPos.position.x, playerPos.position.y, 0);
        this.gameObject.SetActive(true);
    }

    private void Destroy() 
    {
        Destroy(this.gameObject);
    }
}
