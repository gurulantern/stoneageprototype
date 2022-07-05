using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public abstract class Gatherable : MonoBehaviour
{
    //[SerializeField] protected GameEvent _interactEvent;
    //[SerializeField] UnityEvent _unityEvent;
    [SerializeField] protected Source _resourceRef;
    [SerializeField] protected int resourceTotal;
    [SerializeField] protected int harvestTrigger;
    [SerializeField] protected int resourceTaken;
    [SerializeField] protected int resourceRemaining;
    protected int i;
    protected bool playerNear; 
    protected string clickedTag;
    Animator animator;

    protected virtual void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        i = 0;
    }
    protected void OnTriggerEnter2D(Collider2D other) {
        
        if (other.gameObject.GetComponent<CharControllerMulti>().IsMine && i == 0) {
            playerNear = true;
            i++;
        } else {
            Debug.Log(other.gameObject.tag);
        }
    }

    protected void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.GetComponent<CharControllerMulti>().IsMine && i == 1) {
            playerNear = false;
            i = 0 ;
        }
    }

    protected virtual void Harvest()
    {
        Debug.Log("Harvest is firing with playerNear = " + playerNear + " ResourceRemaining = " + resourceRemaining);
        if (playerNear == true && resourceRemaining > 0) {
            gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
            harvestTrigger ++;
            resourceRemaining = Mathf.Clamp(resourceRemaining - resourceTaken, 0, resourceTotal);
            Debug.Log("Gathered invoked and foodRemaining = " + resourceRemaining);
        }
    }
}
