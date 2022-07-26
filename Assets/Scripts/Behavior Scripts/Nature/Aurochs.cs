using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Aurochs : Gatherable
{
    // Clients store twenty states with "playback" information from the server. This
    // array contains the official state of this object at different times according to
    // the server.
    [SerializeField]
    protected AurochsState[] proxyStates = new AurochsState[20];

    // Keep track of what slots are used
    protected int proxyStateCount;

    /// Cached transform
    protected Transform myTransform;
    /// The change in position in the most recent frame. Applies
    /// to all sessions including the owner
    public Vector3 LocalPositionDelta
    {
        get
        {
            return localPositionDelta;
        }
    }
    private Vector3 localPositionDelta;

    /// The position of this transform in the previous frame
    private Vector3 prevLocalPosition;

    private Vector3 finalDestination;
    /// Synchronized object state
    [System.Serializable]
    protected struct AurochsState
    {
        public double timestamp;
        public Vector2 pos;
        public Vector2 vel;
        public Colyseus.Schema.MapSchema<string> attributes;
    }
    [SerializeField] private bool alive;
    [SerializeField] Transform target;
    [SerializeField] private NavMeshAgent agent;
    Vector2 lookDirection = new Vector2(1,0);
    private Animator animator;


    private void Awake()
    {
        /*
        if (alive) {
            this.gameObject.tag = "Live_Aurochs";
            type = "Live_Aurochs";
        } else {
            this.gameObject.tag = "Dead_Aurochs";
            type = "Dead_Aurochs";
        }
        */
        animator = gameObject.GetComponent<Animator>();
        myTransform = transform;
    }

    void Start()
    {
        target = AurochsController.Instance.aurochsSpawnPoints[AurochsController.Instance.currentSpawn + 1].transform;
        if (alive) {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.SetDestination(target.position);
        }
    }

    private void Update() {
        if(alive) {
            localPositionDelta = myTransform.localPosition - prevLocalPosition;
            prevLocalPosition = myTransform.localPosition;

            if(!Mathf.Approximately(localPositionDelta.x, 0.0f))
            {
                lookDirection.Set(localPositionDelta.x, 0);
                lookDirection.Normalize();
            }

            animator.SetFloat("Look X", lookDirection.x);
            animator.SetFloat("Speed", localPositionDelta.magnitude * 100);
            agent.SetDestination(target.position);

            if (myTransform.localPosition == finalDestination)
            {
                Destroy(this.gameObject);
            }
        }
    }

    public override void SetState(GatherableState state)
    {
        base.SetState(state);
        if (currHarvestTrigger == 0) {
            Destroy(this.gameObject);
        }
    }

    public void ResumeJourney()
    {
        
    }

    public void AvoidPlayer()
    {

    }



    public void Pause()
    {

    }


    //Left click decreases food remaining and triggers the animation for food to disappear
    public virtual void Harvest()
    {
        Destroy(this.gameObject);
    }
}
