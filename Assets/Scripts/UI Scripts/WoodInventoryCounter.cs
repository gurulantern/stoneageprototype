using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class WoodInventoryCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counter;    
    CharController character;
    void Awake() => character.ChangedWood += UpdateText;
    void OnDestroy() => character.ChangedWood -= UpdateText;
    void OnEnable() => UpdateText();
    void OnValidate() => counter = GetComponent<TextMeshProUGUI>();
    void UpdateText() => counter.text = character.wood.ToString();

}
