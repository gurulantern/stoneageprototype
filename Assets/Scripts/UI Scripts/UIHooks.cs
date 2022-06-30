using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;
using UnityEngine.SceneManagement;

public class UIHooks : MonoBehaviour
{
    [SerializeField] Slider _staminaSlider;
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
}
