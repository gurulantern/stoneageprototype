using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings", fileName = "New Game Settings", order = 0)]
public class GameSettings : ScriptableObject
{
    public float roundTime = 240;
    public float paintTime = 60;
    public float tireRate = .05f;

    //public Dictionary<string, StoneAgeTeam> team1;
}
