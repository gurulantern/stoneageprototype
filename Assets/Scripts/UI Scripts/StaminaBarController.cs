using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarController : MonoBehaviour
{
    public Slider staminaSlider { get; set; }
    public CharControllerMulti character;        
    void Awake()
    {
        staminaSlider = GetComponent<Slider>();
        character = GetComponent<CharControllerMulti>();
        staminaSlider.value = character.maxStamina;
    }
    
    void FixedUpdate()
    {
        if (character.IsMine) //GameController.Instance.gamePlaying)
        {
            Debug.Log(character.currentStamina);
            staminaSlider.value = character.currentStamina;
        }
    }
}
