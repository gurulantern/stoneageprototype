using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressContainer : MonoBehaviour
{
    public List<Progress> progresses;

    [SerializeField]
    private RectTransform rectTransform;

    public void SetProgress(int teamIndex, int type, string scorableProg)
    {
        if (type == 1) {
            progresses[1].SetProgress(teamIndex, type, scorableProg);
            progresses[1].gameObject.SetActive(true);
        }
        progresses[0].SetProgress(teamIndex, type, scorableProg);
    }

    public void UpdatePosition(Vector2 position)
    {
        rectTransform.position = position;
    }
}
