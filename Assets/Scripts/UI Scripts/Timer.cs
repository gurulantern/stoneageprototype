using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public static Timer instance { get; private set;}
    public Image timerMask;
    float originalSize;
    void Awake()
    {
        instance = this;
    }

     void Start()
    {
        originalSize = timerMask.rectTransform.rect.width;
    }

    public void DecrementTime(float decrement) 
    {
        timerMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize * decrement);
    }
}
