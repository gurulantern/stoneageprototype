using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gatherable : MonoBehaviour
{
    protected int i;
    protected bool playerNear; 

    private void Start() {
        i = 0;
    }
    protected void OnTriggerEnter2D(Collider2D other) {
        
        if (other.gameObject.CompareTag("Player") && i == 0) {
            playerNear = true;
            Debug.Log("player near");
            i++;
        } 
    }

    protected void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player") && i == 1) {
            playerNear = false;
            i = 0 ;
        }
    }
}
