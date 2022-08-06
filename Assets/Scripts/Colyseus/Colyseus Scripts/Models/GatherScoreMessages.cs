using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colyseus;

[System.Serializable]
public class EntityMessage
{
    public string entityID;
}

public class ObjectInitMessage
{
    public string objectID;
}

public class ObjectGatheredMessage
{
    public string gatheredObjectID;
    public string gatheringStateID;
    public string gatheredObjectType;
    public string gatherOrSpend = "gather";
}

partial class Robbable : MonoBehaviour
{
    public struct StoneAgeStealMessage
    {
        public string robber;
        //public int stolenType;
        //public int stolenAmount;
        public bool isRFC;
    }

    public struct StoneAgeGiveMessage
    {
        public string giver;
        public int type;
        public int amount;
        public bool isRFC;
    }
}

public class StoneAgeScoreMessage
{
    public string teamIndex;
    public string scoreType;
    public string updatedScore;
    public string scoreItem;
}

public class StoneAgeUnlockMessage{
    public string teamIndex;
    public string createUnlocked;
}