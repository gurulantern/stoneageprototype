using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Slider timerSlider;
    
    public void SetTime(float time)
    {
        timerSlider.maxValue = time;
        timerSlider.value = timerSlider.maxValue;
    }

    public void DecrementTime(float decrement) 
    {
        timerSlider.value = timerSlider.maxValue - decrement;
    }
}
