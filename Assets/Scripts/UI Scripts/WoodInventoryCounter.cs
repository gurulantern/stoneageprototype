using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class WoodInventoryCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counter;
    [SerializeField] PlayerInventory _playerInventory;

    void Awake() => _playerInventory.ChangedWood += UpdateText;
    void OnDestroy() => _playerInventory.ChangedWood -= UpdateText;
    void OnEnable() => UpdateText();
    void OnValidate() => counter = GetComponent<TextMeshProUGUI>();
    void UpdateText() => counter.text = _playerInventory.wood.ToString();

}
