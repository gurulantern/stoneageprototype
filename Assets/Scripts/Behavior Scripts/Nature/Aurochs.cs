using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Aurochs : Gatherable
{
    public Transform penTransfrom;
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
    [SerializeField] private bool alive;
    [SerializeField] private NavMeshAgent agent;
    Vector2 lookDirection = new Vector2(1,0);
    private Animator animator;
    private IEnumerator findRest;
    [SerializeField] private AurochsScareable scareTrigger;
    [SerializeField] private BoxCollider2D domTrigger;


    private void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
        myTransform = transform;
        typeToGive = "meat";
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
                domTrigger.offset = new Vector2(.16f, 0.5f);
            } else {
                scareTrigger.SetOffset(0.5f);
                domTrigger.offset = new Vector2(-.16f, 0.5f);
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

    protected override void UpdateStateForView()
    {
        base.UpdateStateForView();
    }

    protected override void UpdateViewFromState()
    {
        base.UpdateViewFromState();
        escape = new Vector2(_state.xPos, _state.yPos);
        Debug.Log($"Setting a new destination from state with destination: {escape}.");
        if (alive) {
            AvoidPlayer(escape, true);
        }
    }

    public void ResumeJourney()
    {
        agent.speed = 1.5f;
        agent.SetDestination(finalDestination);
    }

    public void AvoidPlayer(Vector2 newDestination, bool remote)
    {
        if (findRest != null) 
        {
            StopCoroutine(findRest);
        }

        if (remote == false) 
        {
            //Debug.Log($"Setting a new destination in the state for remote - {newDestination}");
            ColyseusManager.NetSend("animalInteraction", new object[]{ this._state.id, newDestination.x, newDestination.y });
        }

        escape = newDestination;
        agent.SetDestination(newDestination);
        agent.speed = 2f;
        distanceFromNewDestination = Vector2.Distance(this.gameObject.transform.position, newDestination);
        findRest = FindRest();
        StartCoroutine(findRest);
    }

    IEnumerator FindRest()
    {
        yield return new WaitForSeconds(5f);

        ResumeJourney();

        //Debug.Log("Pause coroutine finished");
        yield break; 
    }


    //Left click decreases food remaining and triggers the animation for food to disappear
    public virtual void Harvest()
    {
        Destroy(this.gameObject);
    }

    public void Domesticate()
    {
        agent.enabled = false;
        scareTrigger.gameObject.SetActive(false);
        trigger.gameObject.SetActive(false);
    }
}
