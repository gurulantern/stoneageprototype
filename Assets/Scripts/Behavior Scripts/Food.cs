using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private GameEvent _eaten;
    void OnTriggerEnter2D(Collider2D other) 
    {
        CharController controller = other.GetComponent<CharController>();
        if (controller != null)
        {
            if (controller._playerStamina.currentStamina < controller._playerStamina.maxStamina) 
            {
                _eaten?.Invoke();
                Destroy(gameObject);
            }
        }    
    }
}
