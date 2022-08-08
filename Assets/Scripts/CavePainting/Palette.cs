using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Palette : MonoBehaviour
{
    [SerializeField] 
    private Button[] paintButtons;
    [SerializeField]
    private PaintSetter paintSetter;

    public void SetButtonActive(int button)
    {
        paintButtons[button].gameObject.SetActive(true);
    }

    public void SpawnPaint(GameObject painting)
    {
        paintSetter.SetPainting(painting);
    }
}
