using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScareableTrigger : MonoBehaviour
{
    public Vector2 scarerPosition;
    // Start is called before the first frame update
    void OnTriggerEnter2D(Collider2D other)
    {
        CharControllerMulti entity = other.GetComponent<CharControllerMulti>();
        //scarerPosition = this.gameObject.GetComponentInParent<CharControllerMulti>().gameObject.position;
        if (entity != null)
        {
            //entity.Scared(scarerPosition);
        }
    }
    /*
    void OnTriggerExit2D(Collider2D other)
    {
        CharControllerMulti entity = other.GetComponent<CharControllerMulti>();
        if (entity != null)
        {
            owner.PlayerLeftRange(entity);
        }
    }
    */
}
