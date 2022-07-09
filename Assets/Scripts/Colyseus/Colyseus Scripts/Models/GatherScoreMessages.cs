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
    public string gatherOrScore = "gather";
}

public class ObjectScoredMessage
{
    public string scoredObjectID;
    public string scoringStateID;
    public string gatherOrScore = "score";
}