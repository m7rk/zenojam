using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonUI : MonoBehaviour
{
    public RectTransform itemList;

    public Image nameBkg;
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text descText;

    public TMPro.TMP_Text tutorialText;

    public readonly float FLAVORTEXT_ANIM_SPEED = 7f;
    public readonly float COLOR_TRANS_SPEED = 10f;

    private int prog = 0;
    public string moveText;
    public string pickupText;
    public string equipText;
    public string useText;
    public string ladderText;

    // Start is called before the first frame update
    void Start()
    {

        tutorialText.text = GameState.floorID == 10 ? moveText : "";
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.mousePosition.y < 128)
        {
            itemList.anchoredPosition = Vector2.Lerp(itemList.anchoredPosition, new Vector3(0, 40+16, 0), Time.deltaTime * FLAVORTEXT_ANIM_SPEED);
            nameBkg.color = Color.Lerp(nameBkg.color, new Color(nameBkg.color.r, nameBkg.color.g, nameBkg.color.b, 1f), Time.deltaTime * COLOR_TRANS_SPEED);
            titleText.color = Color.Lerp(titleText.color, new Color(titleText.color.r, titleText.color.g, titleText.color.b, 1f), Time.deltaTime * COLOR_TRANS_SPEED);
            descText.color = Color.Lerp(descText.color, new Color(descText.color.r, descText.color.g, descText.color.b, 1f), Time.deltaTime * COLOR_TRANS_SPEED);
        } else
        {
            itemList.anchoredPosition = Vector2.Lerp(itemList.anchoredPosition, new Vector3(0, 16+16, 0), Time.deltaTime * FLAVORTEXT_ANIM_SPEED);
            nameBkg.color = Color.Lerp(nameBkg.color, new Color(nameBkg.color.r, nameBkg.color.g, nameBkg.color.b, 0f), Time.deltaTime * COLOR_TRANS_SPEED);
            titleText.color = Color.Lerp(titleText.color, new Color(titleText.color.r, titleText.color.g, titleText.color.b, 0f), Time.deltaTime * COLOR_TRANS_SPEED);
            descText.color = Color.Lerp(descText.color, new Color(descText.color.r, descText.color.g, descText.color.b, 0f), Time.deltaTime * COLOR_TRANS_SPEED);
        }
    }

    public void progressMove()
    {
        if(prog != 0)
        {
            return;
        }
        prog = 1;
        tutorialText.text = pickupText;
    }

    public void progressPickup()
    {
        if (prog != 1)
        {
            return;
        }
        prog = 2;
        tutorialText.text = equipText;
    }

    public void progressEquip()
    {
        if (prog != 2)
        {
            return;
        }
        prog = 3;
        tutorialText.text = useText;
    }

    public void progressUse()
    {
        if (prog != 3)
        {
            return;
        }
        prog = 4;
        tutorialText.text = ladderText;
    }


}
