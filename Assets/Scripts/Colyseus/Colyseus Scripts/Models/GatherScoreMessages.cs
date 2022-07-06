using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colyseus;

[System.Serializable]
public class EntityMessage
{
    public string entityID;
}

public class ObjectUsedMessage
{
    public string gatheredObjectID;
    public string gatheringStateID;
    public string gatherOrScore;
}



