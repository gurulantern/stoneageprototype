using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodActions : Gatherable
{
    public override void Gather()
    {
        throw new System.NotImplementedException();
    }
    public PlayerInventory _playerInventory;
    public FoodCollection _caveCollection;

    private bool readyToGather = true;
    public bool ReadyToGather 
    {
        get { return readyToGather; } 
        set { readyToGather = value; }
    }
    [SerializeField] float gatherCooldown;
    private float gatherCooldownRem;
    private bool fruitTreeNear;
    private bool caveNear;

    private void Awake() 
    {
        _playerInventory.food = 0;
        _playerInventory.wood = 0;    
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if (other.gameObject.CompareTag("Fruit Tree"))
        {
            fruitTreeNear = true;
        } else if (other.gameObject.CompareTag("Cave")) 
        {
            Debug.Log("Cave near");
            caveNear = true;
        }
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        if (other.gameObject.CompareTag("Fruit Tree"))
        {
            fruitTreeNear = false;
        } else if (other.gameObject.CompareTag("Cave"))
        {
            Debug.Log("Cave not near");
            caveNear = false;
        }        
    }

    public void StartGather()
    {
        if(fruitTreeNear) 
        {
            animator.SetBool("Gather", true);
            _playerInventory.AddFood();
            gatherCooldownRem = gatherCooldown;
            while (gatherCooldownRem != 0)
            {
                ReadyToGather = false;
                gatherCooldownRem = Mathf.Clamp(gatherCooldownRem - Time.fixedDeltaTime, 0, gatherCooldown);
            }
            ReadyToGather = true;
            Debug.Log("Gathered Food: " + _playerInventory.food + "Ready to Gather: " + ReadyToGather);
        }
    }

    public void StopGather()
    {
        animator.SetBool("Gather", false);
    }

    public void DropOff()
    {
        if(caveNear)
        {            
            Debug.Log("Gathered: "+ _playerInventory.food);
            _caveCollection.AddFood(_playerInventory.food);
            Debug.Log(_caveCollection.foodCount);
            _playerInventory.DropOff();
        }
    }
}
