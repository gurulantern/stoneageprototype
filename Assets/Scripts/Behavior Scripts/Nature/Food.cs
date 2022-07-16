using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Gatherable
{
    public float restoreAmount;

    private void Awake()
    {
        restoreAmount = 20f;
        this.gameObject.tag = "Fruit";
        type = "Fruit";
    }
    public override void PlayerInRange(CharControllerMulti entity)
    {
        PlayerAttemptedUse(entity);
        Debug.Log($"Player is trying to eat {this}");
    }

    public virtual void Harvest()
    {
        Destroy(this.gameObject);
    }
}
