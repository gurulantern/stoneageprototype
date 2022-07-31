using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomTrigger : MonoBehaviour
{
    public AurochsPen owner;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.name + "in the pen");
        Aurochs aurochs = other.GetComponentInParent<Aurochs>();
        if (aurochs != null)
        {
            owner.CaughtAurochs(aurochs);
        }
    }
}
