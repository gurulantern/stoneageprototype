using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintController : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField] 
    private Transform playerTransform;
    public List<GameObject> walls;
    public List<GameObject> paintings;
    public static PaintController Instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public void MakeWalls(int count, int teamIdx)
    {
        for (int i = 0; i < count; i ++) {
            GameObject wall = Instantiate(prefab);
            wall.GetComponent<TeamWall>().SetOwnerTeam(i, playerTransform);
            walls.Add(wall);
        }
    }

    public void ShowTeamWall(int teamIdx)
    {
        walls[teamIdx].GetComponent<TeamWall>().Reveal();
    }

    public void RemotePaint(int type, int teamIdx, float posX, float posY, string flipX, string flipY)
    {
        GameObject newPaint = Instantiate(paintings[type]);
        newPaint.transform.SetParent(walls[teamIdx].transform);
        newPaint.transform.localPosition = new Vector3(posX, posY, 0);
        SpriteRenderer paintSprite = newPaint.GetComponent<SpriteRenderer>();
        paintSprite.flipX = flipX == "True";
        paintSprite.flipY = flipY == "True"; 
        if (type <= 3) {
            paintSprite.color = GameController.Instance.GetTeamColor(type);
        }
    }
}
