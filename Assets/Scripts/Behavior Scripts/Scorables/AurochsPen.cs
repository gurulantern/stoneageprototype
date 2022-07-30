using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AurochsPen : Scorable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void CaughtAurochs(Aurochs aurochs)
    {
        states[1].SetActive(false);
        states[2].SetActive(true);
        aurochs.Domesticate();
        Debug.Log($"Player is near {this}");
        ///DisplayInRangeMessage();
    }
}
