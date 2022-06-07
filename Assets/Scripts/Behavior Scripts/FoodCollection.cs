using UnityEngine;
using System;

[CreateAssetMenu(menuName = "FoodCollection", fileName = "New Food Collection", order = 0)]
public class FoodCollection : ScriptableObject 
{

    [SerializeField] private int foodCount = 0;
    public int FoodCount 
    {
        get { return foodCount; }
    }

}
