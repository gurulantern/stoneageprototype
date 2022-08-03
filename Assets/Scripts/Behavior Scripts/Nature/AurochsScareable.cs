using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AurochsScareable : MonoBehaviour
{
    [SerializeField] private Aurochs _owner;
    private CircleCollider2D _collider;
    private bool remote;
    private CharControllerMulti currentEntity;
    private Vector2 newDestination;
    // Start is called before the first frame update
    void Awake()
    {
        _collider = this.gameObject.GetComponent<CircleCollider2D>();
        _owner = this.gameObject.GetComponentInParent<Aurochs>();
    }

    public void SetOffset(float offset)
    {
        _collider.offset = new Vector2(offset, 0.4f);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("Player")) //|| other.gameObject.CompareTag("Other_Player"))
        {
            currentEntity = other.gameObject.GetComponentInParent<CharControllerMulti>();
            Debug.Log(currentEntity.gameObject.name + "is currently scaring");
            Vector2 scarerPos = currentEntity.transform.position;
            Vector2 myPos = this.gameObject.transform.position;
            newDestination = Vector2.LerpUnclamped(scarerPos, myPos, 3f);
            _owner.AvoidPlayer(newDestination, false);
        }    
    }
}
