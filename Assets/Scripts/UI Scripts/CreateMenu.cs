using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateMenu : MonoBehaviour
{
    [SerializeField] 
    private GameObject[] createButtons;
    [SerializeField] private bool open = false;

    public void UnlockButton(string mostObserved)
    {
        switch(mostObserved) {
            case "Fruit_Tree":
                createButtons[0].SetActive(true);
                break;
            case "Aurochs":
                createButtons[1].SetActive(true);
                break;
            case "Tree":
                createButtons[2].SetActive(true);
                break;
        }
    }

    public void SetButtonActive(int button, bool active)
    {
        createButtons[button].GetComponentInChildren<Button>().interactable = active;
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
