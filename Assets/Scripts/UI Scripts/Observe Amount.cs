using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Observe Amount", fileName = "New Observe Amount", order = 0)]
public class ObserveAmount : ScriptableObject
{
    public float observeCount = 0f;

    private int treeCount;
    private int fruitTreeCount;
    private int grainCount;
}
