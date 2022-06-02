using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Player Stamina", fileName = "New Player Stamina", order = 0)]
public class PlayerStamina : ScriptableObject
{
    public float maxStamina = 100;
    public float currentStamina;
    
    void Awake()
    {
        currentStamina = 100;
    }

}
