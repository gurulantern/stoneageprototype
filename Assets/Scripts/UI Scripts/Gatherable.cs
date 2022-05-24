using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CircleCollider2D))]

public abstract class Gatherable : MonoBehaviour, IPointerClickHandler
{
    public abstract void Gather();
    [SerializeField] GameEvent _onClick;
    protected bool playerNear { get; set; } = false;
    private int i = 0;
    public Animator animator;

    void Start() {
        animator = GetComponent<Animator>();
    }
    //Sets playerNear to true or false depending on whether player is in collider
    public virtual void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player") && i == 0)
        {
            playerNear = true;
            Debug.Log(" Player near is " + playerNear);
            i++;
        }
    }
    public virtual void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player") && i == 1)
        {
            playerNear = false;
            i = 0;
        }    
    }

    //Left click triggers an event and right click triggers observe meter increase
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) 
        {
            _onClick?.Invoke();
        }
    }

}
