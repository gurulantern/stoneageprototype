using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Aurochs : Gatherable
{
    [SerializeField] private bool alive;
    [SerializeField] Transform target;
    [SerializeField] int startSpawn;
    [SerializeField] private NavMeshAgent agent;

    /*
    protected override void Awake()
    {
        base.Awake();
        resourceTotal = (int) _state.foodTotal;
        resourceRemaining = (int) _state.foodTotal; 
        harvestTrigger = (int) _state.harvestTrigger;
        resourceTaken = (int) _state.resourceTaken;
    }
    */
    void Start()
    {
        Debug.Log($"{this} is starting");
        target = GameController.Instance.aurochsSpawnPoints[startSpawn + 1].transform;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.SetDestination(target.position);
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
        agent.SetDestination(target.position);
    }

    /*
    //Left click decreases food remaining and triggers the animation for food to disappear
    protected override void Harvest()
    {
        base.Harvest();
    }
    */
}
