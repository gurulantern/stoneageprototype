using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FoodCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counter;
    [SerializeField] FoodCollection foodCollection;

    void Awake() => foodCollection.Changed += UpdateText;
    void OnDestroy() => foodCollection.Changed -= UpdateText;
    void OnEnable() => UpdateText();
    void OnValidate() => counter = GetComponent<TextMeshProUGUI>();
    void UpdateText() => counter.text = foodCollection.foodCount.ToString();

}
