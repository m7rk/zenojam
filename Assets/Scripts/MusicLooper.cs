using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLooper : MonoBehaviour
{
    // Start is called before the first frame update
    public float startTime;
    // Update is called once per frame
    void Update()
    {
        if(GetComponent<AudioSource>().time >= GetComponent<AudioSource>().clip.length)
        {
            GetComponent<AudioSource>().time = startTime;
            GetComponent<AudioSource>().Play();
        }
    }
}
