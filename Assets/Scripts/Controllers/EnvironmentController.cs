using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;
using UnityEngine.AI;

public class EnvironmentController : MonoBehaviour
{
    private static EnvironmentController instance;
    public NavMeshSurface2d Surface2D;

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
    public int fruitCount = -1, treeCount = -1, fruitTreeCount = -1, aurochsCount = -1, aurochsPen = -1, farms = -1, saplings = -1, fishTraps = -1;

    public Scorable[] scorables;

    public Gatherable[] gatherables;

    // Start is called before the first frame update
    void Awake()
    {   
        gatherables = GetComponentsInChildren<Gatherable>();
        scorables = GetComponentsInChildren<Scorable>();
        instance = this;
    }

    private void OnEnable()
    {
        RoomController.onNetworkGatherableAdd += OnGatherableAdd;
        RoomController.onNetworkScorableAdd += OnScorableAdd;
    }

    private void OnDisable()
    {
        RoomController.onNetworkGatherableAdd -= OnGatherableAdd;
        RoomController.onNetworkScorableAdd -= OnScorableAdd;
    }

    public void UpdateNavMesh()
    {
        Surface2D.UpdateNavMesh(Surface2D.navMeshData);
    }

    private void OnGatherableAdd(GatherableState gatherable)
    {
        GetGatherableByState(gatherable);
    }

    private void OnScorableAdd(ScorableState scorable)
    {
        GetScorableByState(scorable);
        
    }


    public void ObjectScored(ScorableState state, CharControllerMulti usingEntity)
    {
        Scorable scorable = GetScorableByState(state);
        if (scorable != null)
        {
            scorable.OnSuccessfulUse(usingEntity, "");
        }
    }

    public void ObjectGathered(GatherableState state, string type, CharControllerMulti usingEntity)
    {
        Gatherable gatherable = GetGatherableByState(state);
        if (gatherable != null)
        {
            gatherable.OnSuccessfulUse(usingEntity, type);
        }
    }

    public Gatherable GetGatherableByState(GatherableState state)
    {
        gatherables = GetComponentsInChildren<Gatherable>();
        foreach (Gatherable t in gatherables)
        {
            if (!t.ID.Equals(state.id))
            {
                continue;
            } 
            

            t.SetState(state);
    

            return t;
        }

        LSLog.LogError("Room has no reference to an gatherable with ID " + state.id + " but it was requested!");
        return null;
    }

    public Scorable GetScorableByState(ScorableState state)
    {
        scorables = GetComponentsInChildren<Scorable>();
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

    public void InitializeInteractables() 
    {
        foreach (Gatherable g in gatherables)
        {
            switch(g.gameObject.tag) {
                case "Fruit_Tree":
                    g.SetID(fruitTreeCount += 1);
                    break;
                case "Fruit":
                    g.SetID(fruitCount += 1);
                    break;
                case "Tree":
                    g.SetID(treeCount += 1);
                    break;
                case "Aurochs":
                    g.SetID(aurochsCount += 1);
                    break;
            }
            ColyseusManager.Instance.SendObjectInit(g);
        }

        foreach (Scorable s in scorables)
        {
            switch(s.gameObject.tag) {
                case "Aurochs_Pen":
                    s.SetID(aurochsPen += 1);
                    break;
                case "Farm":
                    s.SetID(farms += 1);
                    break;
                case "Sapling":
                    s.SetID(saplings += 1);
                    break;
                case "Fishing_Trap":
                    s.SetID(fishTraps += 1);
                    break;
            }
            ColyseusManager.Instance.SendObjectInit(s);
        }
    }
}

