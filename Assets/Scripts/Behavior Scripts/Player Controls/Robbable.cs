using System.Collections;
using System.Collections.Generic;
using UnityEngine;

partial class Robbable : MonoBehaviour
{
    public string remoteEntityID = string.Empty;

    public void Steal(StoneAgeStealMessage data)
    {
        if (data.isRFC)
        {
            if (TryGetComponent(out CharControllerMulti controller))
            {
                controller.Robbed(data.robber);
            }
        }
    }

    public void Give(StoneAgeGiveMessage data)
    {
        if (data.isRFC)
        {
            if(TryGetComponent(out CharControllerMulti controller))
            {
                controller.Receive(data.giver, data.type, data.amount);
            }
        }
    }
}
