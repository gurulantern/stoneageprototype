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
        switch(icon) {
            case  0:
                _personalFruitCount.text = character.fruit.ToString();
                break;
            case 1:
                _personalMeatCount.text = character.meat.ToString();
                break;
            case 2:
                _personalWoodCount.text = character.wood.ToString();
                break;
            case 3:
                _personalFruitCount.text = character.fruit.ToString();
                _personalSeedsCount.text = character.seeds.ToString();
                break;

        }
    }
}
