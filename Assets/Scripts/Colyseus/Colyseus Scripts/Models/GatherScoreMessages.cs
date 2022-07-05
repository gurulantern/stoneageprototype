using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colyseus;

[System.Serializable]
public class EntityMessage
{
    public string entityID;
}

public class ObjectGatherMessage
{
    public string gatheredObjectID;
    public string gatheringStateID;
}

public class ObjectScoreMessage
{
    public string scoredObjectID;
    public string scoringStateID;
}

