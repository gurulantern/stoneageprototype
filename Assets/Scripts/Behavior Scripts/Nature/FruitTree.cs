using UnityEngine;

public class FruitTree : Tree
{
    public bool unharmed = true;
    private void Awake()
    {
        startingHarvest = _treeStates.Length - 1; 
        colliderOffset.x = -0.019f;
        colliderOffset.y = 0.216f;
        colliderSize.x = .827f;
        colliderSize.y = .397f;
        this.gameObject.tag = "Fruit_Tree"; 
        type = "Fruit_Tree";
        typeToGive = "fruit";
    }


    protected override void UpdateViewFromState()
    {
        base.UpdateViewFromState();
        if (prevHarvestTrigger == 10)
        {
            this.gameObject.tag = "Tree";
            type = "Tree";
            typeToGive = "wood";
        } else if (prevHarvestTrigger == 9) {
            unharmed = false; 
        } else if (prevHarvestTrigger == 1) {
            this.gameObject.GetComponent<BoxCollider2D>().offset = colliderOffset;
            this.gameObject.GetComponent<BoxCollider2D>().size = colliderSize;
        }

    }

    //Left click decreases food remaining and triggers the animation for food to disappear
    public override void Harvest()
    {
        base.Harvest();
    }

    public void ResetFruits()
    {
        this.gameObject.tag = "Fruit_Tree";
        type = "Fruit_Tree";
        typeToGive = "fruit";
        _treeStates[currHarvestTrigger].gameObject.SetActive(false);
        currHarvestTrigger = 16;
        prevHarvestTrigger = 17;
        _treeStates[currHarvestTrigger].gameObject.SetActive(true);

    }
}
