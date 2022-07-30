using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateMenu : MonoBehaviour
{
    [SerializeField] private bool open = false;

    public void ToggleMenu()
    {
        open = !open;
        this.gameObject.SetActive(open);
    }

    public void spawnBlueprint(GameObject blueprint)
    {
        Instantiate(blueprint);
    }
}
