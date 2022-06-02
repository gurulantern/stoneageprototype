using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarController : MonoBehaviour
{
    public Slider staminaSlider { get; set; }
    public PlayerStamina _playerStamina;
        void Awake()
    {
        staminaSlider = GetComponent<Slider>();
        staminaSlider.value = _playerStamina.maxStamina;
    }
    
    void FixedUpdate()
    {
        if (GameController.instance.gamePlaying)
        {
            staminaSlider.value = _playerStamina.currentStamina;
        }
    }
}
