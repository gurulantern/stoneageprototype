using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBlueprint : MonoBehaviour
{
    public GameObject objectBlueprint;

    public void spawnBlueprint()
    {
        Instantiate(objectBlueprint);
    }
}
