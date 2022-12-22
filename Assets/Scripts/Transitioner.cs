using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transitioner : MonoBehaviour
{
    private bool endSceneFlag = false;

    public Action transitionDone;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(endSceneFlag)
        {
            this.transform.position += new Vector3(0, 0 - (Time.deltaTime * 700f), 0f);
            if(this.transform.position.y < 360)
            {

                this.transform.position = new Vector3(480, 360, 0f);
                transitionDone.Invoke();
            }
        } 
        else
        {
            this.transform.position += new Vector3(0, 0 - (Time.deltaTime * 700f), 0f);
        }
    }

    public void endScene(Action onEnd)
    {
        transitionDone = onEnd;
        endSceneFlag = true;
        this.transform.position = new Vector3(480, 360*3, 0f);
    }
}
