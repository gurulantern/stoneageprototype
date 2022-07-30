using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomTrigger : MonoBehaviour
{
    public AurochsPen owner;

    void OnTriggerEnter2D(Collider2D other)
    {
        Aurochs aurochs = other.GetComponent<Aurochs>();
        if (aurochs != null)
        {
            owner.CaughtAurochs(aurochs);
        }
    }
}
