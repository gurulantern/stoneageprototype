using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScareableTrigger : MonoBehaviour
{
    public string entityID = string.Empty;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other != null && other.gameObject.CompareTag("Other_Player"))
        {
            CharControllerMulti entity = other.GetComponentInParent<CharControllerMulti>();
            Debug.Log("Trying to scare " + entity);
            //scarerPosition = this.gameObject.GetComponentInParent<CharControllerMulti>().gameObject.position;
            if (entity != null && !GameController.Instance.AreUsersSameTeam(entity, this.gameObject.GetComponentInParent<CharControllerMulti>()))
            {
                Debug.Log("Scaring " + entity);
                entity.Scared(entityID);
            }
        }
    }
}
