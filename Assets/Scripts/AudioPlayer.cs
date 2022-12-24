using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioClip[] clips;

    public void playByName(string n)
    {
        foreach(var v in clips)
        {
            if(v.name == n)
            {
                FindObjectOfType<AudioSource>().PlayOneShot(v,0.5f);
                return;
            }
        }
    }
}
