using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Vector3 spawn;
    public bool Taken { get; private set; }    
    private void Awake()
    {
        spawn = this.transform.position;
    }
}
