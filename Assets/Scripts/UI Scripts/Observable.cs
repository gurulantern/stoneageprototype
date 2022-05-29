using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public  class Observable : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameEvent _onRightClick;
    protected bool playerNear { get; set; } = false;
    private int i = 0;
    public Animator animator;
     void Start() {
        animator = GetComponent<Animator>();
    }
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
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && playerNear == true) 
        {
            _onRightClick?.Invoke();
        }
    }

}
