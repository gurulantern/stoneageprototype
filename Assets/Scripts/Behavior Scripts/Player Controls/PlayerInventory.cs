using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Player Inventory", fileName = "New Player Inventory", order = 0)]
public class PlayerInventory : ScriptableObject
{
    public int food = 0;
    public int wood = 0;
    public event Action ChangedFood;
    public event Action ChangedWood;


    public void AddFood()
    {
        food += 1;
        ChangedFood?.Invoke();
    }

    public void AddWood()
    {
        wood += 1;
        ChangedWood?.Invoke();
    }

    public void Robbed()
    {
        food -= 1;
        ChangedFood?.Invoke();
    }

    public void DropOff()
    {
        food -= food;
        ChangedFood?.Invoke();
    }

    public void UseWood()
    {
        wood -= wood;
        ChangedWood?.Invoke();
    }
}
