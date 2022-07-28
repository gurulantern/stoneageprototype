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

    public float distanceFromNewDestination;
    [SerializeField]
    private Vector2 escape;
    public Vector3 finalDestination;
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
    private IEnumerator findRest;
    [SerializeField] private AurochsScareable scareTrigger;


    private void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
        myTransform = transform;
    }

    void Start()
    {
        if (alive) {
            finalDestination = AurochsController.Instance.aurochsSpawnPoints[AurochsController.Instance.currentSpawn += 1].spawn;
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.SetDestination(finalDestination);
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

            animator.SetFloat("LookX", lookDirection.x);
            animator.SetFloat("Speed", localPositionDelta.magnitude * 100);
            if (lookDirection.x < .1)
            {
                scareTrigger.SetOffset(-0.5f);
            } else {
                scareTrigger.SetOffset(0.5f);
            }

            if (Vector2.Distance(this.gameObject.transform.position, finalDestination) < 0.5f)
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
        agent.SetDestination(finalDestination);
    }

    public void AvoidPlayer(Vector2 newDestination)
    {
        if (findRest != null) 
        {
            StopCoroutine(findRest);
        }
        escape = newDestination;
        agent.SetDestination(newDestination);
        distanceFromNewDestination = Vector2.Distance(this.gameObject.transform.position, newDestination);
        findRest = FindRest();
        StartCoroutine(findRest);
    }

    IEnumerator FindRest()
    {
        while (Vector2.Distance(this.gameObject.transform.position, escape) > 0.5f)
        {
            Debug.Log("running away");
            yield return null;
        }

        Debug.Log($"{this.gameObject.name} escaped and is resting");

        yield return new WaitForSeconds(2.5f);

        ResumeJourney();

        Debug.Log("Pause coroutine finished");
        yield break; 
    }


    //Left click decreases food remaining and triggers the animation for food to disappear
    public virtual void Harvest()
    {
        Destroy(this.gameObject);
    }
}
