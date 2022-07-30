using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateMenu : MonoBehaviour
{
    [SerializeField] private bool open = false;
    [SerializeField] Button _farm;
    [SerializeField] Button _pen;
    [SerializeField] Button _sapling;
    [SerializeField] Button _tools;

    public void ToggleMenu()
    {
        open = !open;
        this.gameObject.SetActive(open);
    }
}
