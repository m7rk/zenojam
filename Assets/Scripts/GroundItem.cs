using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundItem : MonoBehaviour
{
    // this is just an item on the ground the hero can pick up
    public List<string> itemName;
    public List<Sprite> itemSprite;

    public void setItemType(string item)
    {
        GetComponent<SpriteRenderer>().sprite = itemSprite[itemName.FindIndex(a => a == item)];
    }
}
