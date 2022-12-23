using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    public List<GameObject> scenes;
    int scenePointer = 0;
    public string whereToGo;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            scenePointer++;
            if (scenePointer >= scenes.Count)
            {
                GameState.floorID = 10;
                SceneManager.LoadScene(whereToGo);
                return;
            }
            else
            {
                foreach (var v in scenes)
                {
                    v.SetActive(false);
                }
                scenes[scenePointer].SetActive(true);
            }
        }
    }
}
