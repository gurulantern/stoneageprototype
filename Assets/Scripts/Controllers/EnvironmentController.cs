using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    private static EnvironmentController instance;

    public static EnvironmentController Instance
    {
        get
        {
            if (instance == null)
            {
                LSLog.LogError("No EnvironmentController in scene!");
            }

            return instance;
        }
    }

    [SerializeField]
    private Scorable[] scorables;

    [SerializeField]
    private Gatherable[] gatherables;

    // Start is called before the first frame update
    void Awake()
    {   
        gatherables = GetComponentsInChildren<Gatherable>();
        scorables = GetComponentsInChildren<Scorable>();
    }

    public void ObjectScored(ScorableState state, NetworkedEntity usingEntity)
    {
        Scorable scorable = GetScorableByState(state);
        if (scorable != null)
        {
            scorable.OnSuccessfulUse(usingEntity);
        }
    }

    public void ObjectGathered(GatherableState state, NetworkedEntity usingEntity)
    {
        Gatherable gatherable = GetGatherableByState(state);
        if (gatherable != null)
        {
            gatherable.OnSuccessfulUse(usingEntity);
        }
    }

    public Gatherable GetGatherableByState(GatherableState state)
    {
        foreach (Gatherable t in gatherables)
        {
            if (!t.ID.Equals(state.id))
            {
                continue;
            }

            //This gatherable has the correct ID but it has not yet been given a state, so correct that!
            if (t.State == null)
            {
                t.SetState(state);
            }

            return t;
        }

        LSLog.LogError("Room has no reference to an gatherable with ID " + state.id + " but it was requested!");
        return null;
    }

    public Scorable GetScorableByState(ScorableState state)
    {
        foreach (Scorable t in scorables)
        {
            if (!t.ID.Equals(state.id))
            {
                continue;
            }

            //This gatherable has the correct ID but it has not yet been given a state, so correct that!
            if (t.State == null)
            {
                t.SetState(state);
            }

            return t;
        }

        LSLog.LogError("Room has no reference to a scorable with ID " + state.id + " but it was requested!");
        return null;
    }
}
