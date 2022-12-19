using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonUI : MonoBehaviour
{
    public RectTransform itemList;
    public readonly float FLAVORTEXT_ANIM_SPEED = 7f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.mousePosition.y < 128)
        {
            itemList.anchoredPosition = Vector2.Lerp(itemList.anchoredPosition, new Vector3(0, 40+16, 0), Time.deltaTime * FLAVORTEXT_ANIM_SPEED);
        } else
        {
            itemList.anchoredPosition = Vector2.Lerp(itemList.anchoredPosition, new Vector3(0, 16+16, 0), Time.deltaTime * FLAVORTEXT_ANIM_SPEED);
        }
    }
}
