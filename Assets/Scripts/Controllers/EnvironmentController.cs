using System;
using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;
using UnityEngine.AI;

public class EnvironmentController : MonoBehaviour
{
    private static EnvironmentController instance;
    public NavMeshSurface2d Surface2D;

    [SerializeField]
    private List<GameObject> scorablePrefabs;
    private int scorableSelector;

    public delegate void InitProgresses(Scorable scorable);
    /// Event for when progresses need to be added.
    public static InitProgresses initProgresses;  

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
    public int fruitCount = -1, treeCount = -1, fruitTreeCount = -1, aurochsCount = -1, aurochsPen = -1, farms = -1, saplings = -1, fishTraps = -1, caves = -1;

    public Scorable[] scorables;
    [SerializeField] private Transform scoreTransform;

    public Gatherable[] gatherables;
    [SerializeField] private Transform gatherTransform;

    private NavMeshData NavMeshData;

    //public delegate void InitializedObject(Scorable scorable);
    //public static event InitializedObject initObject;

    // Start is called before the first frame update
    void Awake()
    {   
        NavMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(NavMeshData);
        gatherables = GetComponentsInChildren<Gatherable>();
        scorables = GetComponentsInChildren<Scorable>();
        instance = this;
    }

    private void OnEnable()
    {
        RoomController.onNetworkGatherableAdd += OnGatherableAdd;
        RoomController.onNetworkScorableAdd += OnScorableAdd;

        BlueprintScript.createObject += OnCreateObject;
    }

    private void OnDisable()
    {
        RoomController.onNetworkGatherableAdd -= OnGatherableAdd;
        RoomController.onNetworkScorableAdd -= OnScorableAdd;

        BlueprintScript.createObject -= OnCreateObject;
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
        if (GetScorableByState(scorable) ==  null) {
            Debug.Log("Instantiating " + scorable.id);
            switch(scorable.scorableType) {
                case("FARM"):
                {
                    scorableSelector = 0;
                    break;
                }
                case("AUROCHS_PEN"):
                {
                    scorableSelector = 1;
                    break;
                }
                case("SAPLING"):
                {
                    scorableSelector = 2;
                    break;
                }
            }
            GameObject instance = Instantiate(scorablePrefabs[scorableSelector], new Vector2(scorable.xPos, scorable.yPos), new Quaternion(0, 0, 0 , 0));
            instance.transform.SetParent(scoreTransform);
            Scorable newScorable = instance.GetComponent<Scorable>();
            switch(instance.gameObject.tag) {
                case "Aurochs_Pen":
                    newScorable.SetID(aurochsPen += 1);
                    break;
                case "Farm":
                    newScorable.SetID(farms += 1);
                    break;
                case "Sapling":
                    newScorable.SetID(saplings += 1);
                    break;
                case "Fishing_Trap":
                    newScorable.SetID(fishTraps += 1);
                    break;
            }
            newScorable.SetState(scorable);
            newScorable.SetOwnerTeam((int)scorable.teamId);
            initProgresses?.Invoke(GetScorableByState(scorable));
            UpdateNavMesh();
            Debug.Log("Scorable was instantiated.");
        } 
    }

    private void OnCreateObject(int type, float cost, Scorable scorable)
    {               
        scorable.transform.SetParent(scoreTransform);
        InitializeNewInteractable(scorable, scorable.gameObject.transform.position);
        UpdateNavMesh();
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

        LSLog.LogError("Room has no reference to a gatherable with ID " + state.id + " but it was requested!");
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

            t.SetState(state);


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
                case "Cave":
                    s.SetID(caves += 1);
                    break;
            }
            ColyseusManager.Instance.SendObjectInit(s, s.gameObject.transform.position.x, s.gameObject.transform.position.y, s.ownerTeam);
        }
    }

    public void InitializeNewInteractable(Scorable s, Vector2 position)
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
            ColyseusManager.Instance.SendObjectInit(s, position.x, position.y, s.ownerTeam);
    }

    public void CheckIfFinished(string entityID, string scorableID, int teamIndex, float woodPaid, float seedsPaid)
    {
        foreach (Scorable s in scorables) {
            if (s.ID == scorableID) {
                s.CheckIfFinished(entityID, teamIndex.ToString(), woodPaid, seedsPaid);
                return;
            }
        }
    }

    public void FinishObject(string ownerID, string scorableID)
    {
        foreach (Scorable s in scorables) {
            if (s.ID == scorableID) {
                s.FinishObject(ownerID);
                return;
            }
        }
    }
}

