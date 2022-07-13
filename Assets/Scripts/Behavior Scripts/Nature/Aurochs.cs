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
    protected Vector3 localPositionDelta;

    /// The position of this transform in the previous frame
    protected Vector3 prevLocalPosition;

    /// Synchronized object state
    [System.Serializable]
    protected struct AurochsState
    {
        public double timestamp;
        public Vector2 pos;
        public Vector2 vel;
        public int food;
        public Colyseus.Schema.MapSchema<string> attributes;
    }
    [SerializeField] private bool alive;
    [SerializeField] Transform target;
    [SerializeField] int startSpawn;
    [SerializeField] private NavMeshAgent agent;

    void Start()
    {
        target = GameController.Instance.aurochsSpawnPoints[startSpawn + 1].transform;
        if (alive) {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.SetDestination(target.position);
        }
    }

    void FixedUpdate()
    {
        /*
        if(!Mathf.Approximately(localPositionDelta.x, 0.0f) || !Mathf.Approximately(localPositionDelta.y, 0.0f))
        {
            lookDirection.Set(localPositionDelta.x, localPositionDelta.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", localPositionDelta.magnitude * 100);
        */
    }
    private void Update() {
        if(alive) {
            agent.SetDestination(target.position);
        }
    }

    public override void SetState(GatherableState state)
    {
        base.SetState(state);
    }


    //Left click decreases food remaining and triggers the animation for food to disappear
    public virtual void Harvest()
    {
        Destroy(this.gameObject);
    }
}
