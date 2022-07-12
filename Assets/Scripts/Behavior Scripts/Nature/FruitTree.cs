using UnityEngine;

public class FruitTree : Tree
{
    protected override void Awake()
    {
        base.Awake();
        /*
        foodTotal = (int)_state.foodTotal;
        foodRemaining = (int)_state.foodTotal; 
        foodTaken = (int)_state.resourceTaken;
        seedsTotal = (int)_state.seedsTotal;
        seedsTaken = (int)_state.seedsTaken;
        seedsRemaining = (int)_state.seedsTotal;
        harvestTrigger = (int)_state.harvestTrigger;
        */
    }
    //Left click decreases food remaining and triggers the animation for food to disappear

    public override void SetState(GatherableState state)
    {
        base.SetState(state);
        prevHarvestTrigger = (int)_state.harvestTrigger;
    }
    protected override void Harvest()
    {
        _treeStates[prevHarvestTrigger].gameObject.SetActive(false);
        prevHarvestTrigger = (int)_state.harvestTrigger;
        _treeStates[prevHarvestTrigger].gameObject.SetActive(true);

        /*
        if (gameObject.tag == "Tree") {
            base.Harvest();
        } else {
            Debug.Log($"Harvest is firing with playerNear = {playerNear}, foodRemaining = {foodRemaining}, seedsRemaing = {seedsRemaining}");
            if (playerNear == true && foodRemaining > 1) {
                gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
                harvestTrigger ++;
                foodRemaining = Mathf.Clamp(foodRemaining - foodTaken, 0, foodTotal);
                seedsRemaining = Mathf.Clamp(seedsRemaining - seedsTaken, 0, seedsTotal);
                Debug.Log($"Gathered invoked and foodRemaining = {foodRemaining}");
            } else if (playerNear == true && foodRemaining == 1) {
                gameObject.GetComponent<Animator>().SetTrigger("Harvest " + harvestTrigger.ToString());
                harvestTrigger ++;
                foodRemaining = Mathf.Clamp(foodRemaining - foodTaken, 0, foodTotal);
                seedsRemaining = Mathf.Clamp(seedsRemaining - seedsTaken, 0, seedsTotal);
                gameObject.tag = "Tree";       
            }
        }
        */
    }

    protected override void UpdateStateForView()
    {
        base.UpdateStateForView();

    }

    public override void OnSuccessfulUse(NetworkedEntityView entity)
    {
        Harvest();
        entity.gameObject.GetComponent<CharControllerMulti>().StartGather();
    }

}
