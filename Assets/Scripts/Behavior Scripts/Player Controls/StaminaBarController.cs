using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarController : MonoBehaviour
{
    public Slider staminaSlider { get; set; }
    CharController character;        
    void Awake()
    {
        staminaSlider = GetComponent<Slider>();
        character = GetComponent<CharController>();
        staminaSlider.value = character.maxStamina;
    }
    
    void FixedUpdate()
    {
        if (GameController.instance.gamePlaying)
        {
            staminaSlider.value = character.currentStamina;
        }
    }
}
