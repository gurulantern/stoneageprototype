using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateMenu : MonoBehaviour
{
    [SerializeField] private Button[] createButtons;
    [SerializeField] private bool open = false;

    public void SetButtonActive(int button, bool active)
    {
        createButtons[button].interactable = active;
    }

    public void ToggleMenu()
    {
        open = !open;
        this.gameObject.SetActive(open);
    }

    public void spawnBlueprint(GameObject blueprint)
    {
        Instantiate(blueprint);
        this.gameObject.SetActive(false);
    }
}
