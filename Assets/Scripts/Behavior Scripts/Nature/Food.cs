using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private GameEvent _eaten;
    void OnTriggerEnter2D(Collider2D other) 
    {
        CharControllerMulti controller = other.GetComponent<CharControllerMulti>();
        if (controller.IsMine)
        {
            if (controller.currentStamina < controller.maxStamina) 
            {
                _eaten?.Invoke();
                Destroy(gameObject);
            }
        }    
    }
}
