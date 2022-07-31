using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AurochsPen : Scorable
{
    public virtual void CaughtAurochs(Aurochs aurochs)
    {
        states[1].SetActive(false);
        states[2].SetActive(true);
        aurochs.Domesticate();
    }
}
