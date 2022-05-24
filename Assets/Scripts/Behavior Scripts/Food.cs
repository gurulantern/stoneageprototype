using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public int food = 20;
    void OnTriggerEnter2D(Collider2D other) 
    {
        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller != null)
        {
            if (controller._playerStamina.currentStamina < controller._playerStamina.maxStamina) 
            {
                controller.FoodStamina(food);
                Destroy(gameObject);
            }
        }    
    }
}
