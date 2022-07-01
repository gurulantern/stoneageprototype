using System;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;

[Serializable]
public class StoneAgeTeam
{
    public int teamIndex = -1;
    public List<string> clientsOnTeam = new List<string>();
    public bool steal = false;
    public bool scare = false;
    public bool create = false; 



    public void SetOptions1(string option, string toggle) 
    {
        switch(option) 
        {
            case "steal":
                steal = bool.Parse(toggle);    
                break;
            case "scare":
                scare = bool.Parse(toggle);    
                break;
            case "create":
                create = bool.Parse(toggle);    
                break;
            default:
                Debug.Log("No team options to set");
                break;
        }
    }

    public bool AddPlayer(string clientID)
    {
        if (ContainsClient(clientID))
        {
            LSLog.LogError($"Team {teamIndex} already has a client with ID {clientID}! Will not add");
            return false;
        }

        clientsOnTeam.Add(clientID);
        return true;
    }

    public bool RemovePlayer(string clientID)
    {
        if (!ContainsClient(clientID))
        {
            LSLog.LogError($"Team {teamIndex} does not have a client with ID {clientID}! Will not remove them");
            return false;
        }

        clientsOnTeam.Remove(clientID);
        return true;
    }

    public bool ContainsClient(string clientID)
    {
        return clientsOnTeam.Contains(clientID);
    }
}
