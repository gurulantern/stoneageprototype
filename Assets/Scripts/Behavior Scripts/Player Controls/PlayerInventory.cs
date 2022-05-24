using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Inventory", fileName = "New Player Inventory", order = 0)]
public class PlayerInventory : ScriptableObject
{
    public int food = 0;
    public int wood = 0;

    void Awake()
    {
        food = 0;
        wood = 0;
    }

    public void AddFood()
    {
        food += 1;
    }

    public void AddWood()
    {
        wood += 1;
    }

    public void DropOff()
    {
        food -= food;
    }

    public void UseWood()
    {
        wood -= wood;
    }
}
