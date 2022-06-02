using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodInventoryCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counter;
    [SerializeField] PlayerInventory _playerInventory;

    void Awake() => _playerInventory.ChangedFood += UpdateText;
    void OnDestroy() => _playerInventory.ChangedFood -= UpdateText;
    void OnEnable() => UpdateText();
    void OnValidate() => counter = GetComponent<TextMeshProUGUI>();
    void UpdateText() => counter.text = _playerInventory.food.ToString();

}
