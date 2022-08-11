using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Palette : MonoBehaviour
{
    [SerializeField] 
    private Button[] paintButtons;
    [SerializeField]
    private GameObject[] painters; 
    [SerializeField]
    private Transform wall;
    [SerializeField] 
    public Camera _camera;

    public void SetWall(TeamWall myWall)
    {
        wall = myWall.gameObject.transform;
    }

    public void SetButtonActive(int button)
    {
        paintButtons[button].gameObject.SetActive(true);
    }

    // Uses the index assigned in the inspector to grab the Painter from the array in Palette. 
    // Sets the color of the blank 4 player painters and ignores colors for other painters
    public void SpawnPainter(int typeIndex)
    {
        GameObject paint = Instantiate(painters[typeIndex]) as GameObject;
        PaintSetter painter = paint.GetComponent<PaintSetter>();
        painter._camera = _camera;
        painter.type = typeIndex;
        if (typeIndex <= 3) {
            paint.GetComponent<SpriteRenderer>().color = GameController.Instance.GetTeamColor(typeIndex);
        }
    }
}
