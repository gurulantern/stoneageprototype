using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Cave : Gatherable
{
    public override void Gather() 
    {
        throw new System.NotImplementedException();
    }
    private int i = 0;
    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && i == 0)
        {
            playerNear = true;
            Debug.Log(" Player near is " + playerNear);
            i++;
        }
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && i == 1)
        {
            playerNear = false;
            Debug.Log("Player exited");
            i = 0;
        }    
    }
}
