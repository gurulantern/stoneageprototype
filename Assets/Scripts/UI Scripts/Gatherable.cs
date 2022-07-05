using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public abstract class Gatherable : MonoBehaviour
{
    //[SerializeField] protected GameEvent _interactEvent;
    //[SerializeField] UnityEvent _unityEvent;
    protected int i;
    protected bool playerNear; 
    protected string clickedTag;

    protected virtual void Awake() {

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
}
