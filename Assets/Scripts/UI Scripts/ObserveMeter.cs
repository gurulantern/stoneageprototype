using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObserveMeter : MonoBehaviour
{
    public Slider observeSlider { get; set; }
    float originalSize;
    public GameObject createAction;
    
    void Awake()
    {
        observeSlider = GetComponent<Slider>();
    }
    
    public void SetMeter(int teamObserve)
    {
        observeSlider.value = teamObserve;
    }

    public void Unlock(string mostObserved)
    {
        if (GameController.Instance.create) {
            if (observeSlider.value == 100f) {
                Unlock(mostObserved);
                createAction.SetActive(true);
            } 
        }
    }
}
