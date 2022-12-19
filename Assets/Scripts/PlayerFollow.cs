using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public readonly float FOLLOW_SPEED = 3f;
    public readonly float VECTOR_H_OFFSET = -0.3f;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector3.Scale(new Vector3(1,1,0),Vector3.Lerp(this.transform.position, (VECTOR_H_OFFSET * Vector3.up) + player.transform.position,Time.deltaTime * FOLLOW_SPEED));
    }
}
