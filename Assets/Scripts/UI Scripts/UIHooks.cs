using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class UIHooks : MonoBehaviour
{
    [SerializeField] Slider _staminaSlider;
    [SerializeField] Slider _observeSlider;
    [SerializeField] TextMeshProUGUI _personalFruitCount;
    [SerializeField] TextMeshProUGUI _personalMeatCount;
    [SerializeField] TextMeshProUGUI _personalWoodCount;
    [SerializeField] TextMeshProUGUI _personalSeedsCount;
    [SerializeField] TextMeshProUGUI _personalFishCount;
    [SerializeField] CreateMenu _createMenu;

    
    public CharControllerMulti character;
    // Start is called before the first frame update

    public void Initialize() {
        character = this.gameObject.GetComponent<CharControllerMulti>();
        _staminaSlider.value = character.maxStamina;

        character.ChangedResource += UpdateText;
    }
    
    void FixedUpdate()
    {
        if (character.IsMine && character != null) //&& GameController.Instance.gamePlaying)
        {
            _staminaSlider.value = character.currentStamina;
        }
    }

    void OnDestroy() 
    {
        if(character != null)
        {
            character.ChangedResource -= UpdateText;
        }
    }
    void OnEnable() 
    {
        BlueprintScript.createObject += ChargePlayer;
        //character.ChangedResource += UpdateText;
    }

    void OnDisable()
    {
        BlueprintScript.createObject -= ChargePlayer;
    }

    void UpdateText(int icon) 
    {
        _personalFruitCount.text = character.fruit.ToString();
        _personalMeatCount.text = character.meat.ToString();
        _personalWoodCount.text = character.wood.ToString();
        _personalSeedsCount.text = character.seeds.ToString();
        //_personalFishCount.text = character.fish.ToString();
    }

    public void ToggleCreate()
    {
        _createMenu.ToggleMenu();
    }

    public void ToggleMenuButton(int button, bool active)
    {
        _createMenu.SetButtonActive(button, active);
    }

    public void ChargePlayer(int type, float cost, Scorable scorable)
    {
        character.SubtractResource(type, cost);
    }
}
