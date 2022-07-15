using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObserveMeter : MonoBehaviour
{
    public Slider observeSlider { get; set; }
    public float increment;
    float originalSize;
    public GameObject createAction;
    
    void Awake()
    {
        observeSlider = GetComponent<Slider>();
    }
    
    public void Increment()
    {

        observeSlider.value += increment;
        if (observeSlider.value >= 90f) {
            createAction.SetActive(true);
        }       
    }
}
