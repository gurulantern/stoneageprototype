using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Collider that tells an <see cref="InteractableState"/> if/when a <see cref="NetworkedEntity"/> enters/exits
/// </summary>
public class InteractableTrigger : MonoBehaviour
{
    public Interactable owner;

    void OnTriggerEnter2D(Collider2D other)
    {
        CharControllerMulti entity = other.GetComponent<CharControllerMulti>();
        if (entity != null)
        {
            owner.PlayerInRange(entity);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        CharControllerMulti entity = other.GetComponent<CharControllerMulti>();
        if (entity != null)
        {
            owner.PlayerLeftRange(entity);
        }
    }
}

