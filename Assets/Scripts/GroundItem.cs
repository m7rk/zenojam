using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundItem : MonoBehaviour
{
    // this is just an item on the ground the hero can pick up
    public List<string> allItemName;
    public List<Sprite> allItemSprite;

    public string thisItem;
    public void setItemType(string item)
    {
        GetComponent<SpriteRenderer>().sprite = allItemSprite[allItemName.FindIndex(a => a == item)];
        thisItem = item;
    }

    public bool canDistract(string NPCName)
    {
        switch(thisItem)
        {
            case "Bone": return NPCName == "Ghost";
            case "Seed": return NPCName == "Kenku";
            case "Gem": return NPCName == "Kobold";
            case "Gold": return NPCName == "Knight";
            case "Meat": return NPCName == "Gnoll";
            case "Flower": return NPCName == "Ghost";
        }
        return false;
    }
}
