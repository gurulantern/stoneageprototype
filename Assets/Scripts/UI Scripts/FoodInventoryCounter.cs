using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodInventoryCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counter;
    CharControllerMulti character;
/*
    void Awake() => character.ChangedFood += UpdateText;
    void OnDestroy() => character.ChangedFood -= UpdateText;
    void OnEnable() => UpdateText();
    void OnValidate() => counter = GetComponent<TextMeshProUGUI>();
    void UpdateText() => counter.text = character.food.ToString();
*/
}
