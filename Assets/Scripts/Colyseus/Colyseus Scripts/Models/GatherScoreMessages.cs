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

public class StoneAgeScoreMessage
{
    public string teamIndex;
    public string scoreType;
    public string updatedScore;
}