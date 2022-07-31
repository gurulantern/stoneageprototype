using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomTrigger : MonoBehaviour
{
    public AurochsPen owner;
    [SerializeField]
    private BoxCollider2D _collider;
    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

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
