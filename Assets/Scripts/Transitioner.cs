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
        RectTransform rt = ((RectTransform)this.transform);

        if (endSceneFlag)
        {
            rt.anchoredPosition += new Vector2(0, 0 - (Time.deltaTime * 700f));
            if(rt.anchoredPosition.y < 0)
            {

                rt.anchoredPosition = new Vector3(0, 0, 0f);
                transitionDone.Invoke();
            }
        } 
        else
        {
            rt.anchoredPosition += new Vector2(0, 0 - (Time.deltaTime * 700f));
        }
    }

    public void endScene(Action onEnd)
    {
        RectTransform rt = ((RectTransform)this.transform);
        transitionDone = onEnd;
        endSceneFlag = true;
        rt.anchoredPosition = new Vector3(0, 360, 0f);
    }
}
