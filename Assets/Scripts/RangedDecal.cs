using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedDecal : MonoBehaviour
{
    private Vector3 targetDest;
    private float travelSpeed;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, targetDest, travelSpeed * Time.deltaTime);
        if(Vector3.Distance(this.transform.position,targetDest) < 0.01f)
        {
            Destroy(this.gameObject);
        }
    }

    public void setGoal(Vector3 target)
    {
        targetDest = target;
        targetDest = new Vector3(targetDest.x, targetDest.y, 2);
        travelSpeed = (this.transform.position - target).magnitude / GameState.ACTION_SPEED;
    }
}
