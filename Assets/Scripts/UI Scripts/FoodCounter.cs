using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FoodCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counter;
    Cave cave;
    void Awake() => cave.Changed += UpdateText;
    void OnDestroy() => cave.Changed -= UpdateText;
    void OnEnable() => UpdateText();
    void OnValidate() => counter = GetComponent<TextMeshProUGUI>();
    void UpdateText() => counter.text = cave.FoodCount.ToString();

}
