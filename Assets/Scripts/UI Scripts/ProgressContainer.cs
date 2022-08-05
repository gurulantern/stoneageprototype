using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressContainer : MonoBehaviour
{
    public List<Progress> progresses;

    [SerializeField]
    private RectTransform rectTransform;

    public void UpdatePosition(Vector2 position)
    {
        rectTransform.position = position;
    }
}
