using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodInventoryCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counter;
    CharController character;
    void Awake() => character.ChangedFood += UpdateText;
    void OnDestroy() => character.ChangedFood -= UpdateText;
    void OnEnable() => UpdateText();
    void OnValidate() => counter = GetComponent<TextMeshProUGUI>();
    void UpdateText() => counter.text = character.food.ToString();

}
