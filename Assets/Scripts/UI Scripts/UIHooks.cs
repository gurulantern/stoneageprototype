using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIHooks : MonoBehaviour
{
    [SerializeField] Slider _staminaSlider;
    [SerializeField] Slider _observeSlider;
    [SerializeField] TextMeshProUGUI _personalFruitCount;
    [SerializeField] TextMeshProUGUI _personalMeatCount;
    [SerializeField] TextMeshProUGUI _personalWoodCount;
    [SerializeField] TextMeshProUGUI _personalSeedsCount;
    [SerializeField] TextMeshProUGUI _personalFishCount;

    
    CharControllerMulti character;
    // Start is called before the first frame update
    void Awake()
    {
        character = this.gameObject.GetComponent<CharControllerMulti>();
        _staminaSlider.value = character.maxStamina;
    }
    
    void FixedUpdate()
    {
        if (character.IsMine) //&& GameController.Instance.gamePlaying)
        {
            _staminaSlider.value = character.currentStamina;
        }
    }

    void OnDestroy() 
    {
        character.ChangedResource -= UpdateText;
    }
    void OnEnable() 
    {
        character.ChangedResource += UpdateText;
    }

    void UpdateText(int icon) 
    {
        _personalFruitCount.text = character.fruit.ToString();
        _personalMeatCount.text = character.meat.ToString();
        _personalWoodCount.text = character.wood.ToString();
        _personalSeedsCount.text = character.seeds.ToString();
        //_personalFishCount.text = character.fish.ToString();
    }
}
