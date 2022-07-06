using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collider that tells an <see cref="InteractableState"/> if/when a <see cref="NetworkedEntity"/> enters/exits
/// </summary>
public class InteractableTrigger : MonoBehaviour
{
    public Interactable owner;

    void OnTriggerEnter(Collider other)
    {
        CharControllerMulti entity = other.GetComponent<CharControllerMulti>();
        if (entity != null)
        {
            owner.PlayerInRange(entity);
        }
    }

    void OnTriggerExit(Collider other)
    {
        CharControllerMulti entity = other.GetComponent<CharControllerMulti>();
        if (entity != null)
        {
            owner.PlayerLeftRange(entity);
        }
    }
}
