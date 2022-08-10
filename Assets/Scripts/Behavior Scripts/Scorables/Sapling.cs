using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sapling : Scorable
{
    [SerializeField]
    private int currentState = 0;
    [SerializeField]
    private GameObject treeFab;
    Quaternion blankRot = new Quaternion(0,0,0,0);

    public void Grow()
    {
        if (currentState < 2) {
            states[currentState].SetActive(false);
            currentState += 1;
            states[currentState].SetActive(true);
        } else {
            BecomeTree();
        }
    }

    private void BecomeTree()
    {
        Destroy(this.gameObject);
        GameObject thisTree = Instantiate(treeFab, this.gameObject.transform.position, blankRot);
        thisTree.transform.SetParent(EnvironmentController.Instance.gatherTransform);
    }
}
