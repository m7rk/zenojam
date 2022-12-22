using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedDecal : MonoBehaviour
{
    private Vector3 targetDest;
    public float TRAVEL_SPEED = 3f;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, targetDest, TRAVEL_SPEED * Time.deltaTime);
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 2);
        if(Vector3.Distance(this.transform.position,targetDest) < 0.01f)
        {
            Destroy(this.gameObject);
        }
    }

    public void setGoal(Vector3 target)
    {
        targetDest = target;
    }
}
