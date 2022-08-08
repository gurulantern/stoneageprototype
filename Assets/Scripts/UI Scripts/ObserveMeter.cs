using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObserveMeter : MonoBehaviour
{
    public Slider observeSlider { get; set; }
    float originalSize;
    public GameObject createAction;
    private int reqIncrement;
    
    void Awake()
    {
        observeSlider = GetComponent<Slider>();
    }

    public void UpdateReq(int newReq)
    {
        observeSlider.maxValue = newReq;
        reqIncrement = newReq;
    }
    
    public void SetMeter(int teamObserve)
    {
        observeSlider.value = teamObserve;
        if (observeSlider.value == observeSlider.maxValue) {
            observeSlider.maxValue += reqIncrement; 
        }
    }
}
