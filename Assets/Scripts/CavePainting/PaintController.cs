using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintController : MonoBehaviour
{
    public Transform _cavePaintingRoot;
    public List<TeamWall> teamWalls;
    [SerializeField]
    private GameObject prefab;

    public void AddNewWall(bool myTeam)
    {
        Instantiate(prefab);
        prefab.SetActive(true);
        prefab.transform.SetParent(_cavePaintingRoot);
    }

}
