using UnityEngine;

namespace StoneAge.Interactables
{
    public class FruitTree : Tree
    {
        private void Awake()
        {
            startingHarvest = _treeStates.Length - 1; 
            colliderOffset.x = -0.019f;
            colliderOffset.y = 0.216f;
            colliderSize.x = .827f;
            colliderSize.y = .397f;
            this.gameObject.tag = "Fruit_Tree";
            type = "Fruit_Tree";
        }
        //Left click decreases food remaining and triggers the animation for food to disappear
        public override void Harvest()
        {
            base.Harvest();
            if (prevHarvestTrigger == 8)
            {
                this.gameObject.tag = "Tree";
                type = "Tree";
            } else if (prevHarvestTrigger == 1) {
                this.gameObject.GetComponent<BoxCollider2D>().offset = colliderOffset;
                this.gameObject.GetComponent<BoxCollider2D>().size = colliderSize;
            }
        }
    }
}
