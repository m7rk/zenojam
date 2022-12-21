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

    public readonly float FLAVORTEXT_ANIM_SPEED = 7f;
    public readonly float COLOR_TRANS_SPEED = 10f;

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
}
