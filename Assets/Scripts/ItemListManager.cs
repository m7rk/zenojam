using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemListManager : MonoBehaviour
{
    public GameObject clickableItemBase;
    public List<GameObject> buttons;

    public TMPro.TMP_Text dmg;
    public TMPro.TMP_Text range;
    public TMPro.TMP_Text descriptor;
    public TMPro.TMP_Text tname;

    // Start is called before the first frame update
    public void generate()
    {
        // del old buts
        if(buttons != null)
        {
            foreach(var v in buttons)
            {
                Destroy(v);
            }
        }

        // new buts
        buttons = new List<GameObject>();
        int x = 0;
        foreach(var v in GameState.playerItems)
        {
            int x2 = x;
            var it = Instantiate(clickableItemBase);
            it.transform.SetParent(this.transform);
            it.GetComponent<Image>().sprite = v.image;
            it.transform.localScale = Vector3.one;
            it.transform.localPosition = new Vector3(48*(x - GameState.playerItemIndex), 0, 0);
            it.GetComponent<Button>().onClick.AddListener(() => onClick(x2));
            buttons.Add(it);
            if(x == GameState.playerItemIndex)
            {
                dmg.text = (v.damageLow != v.damageHi) ? v.damageLow + "~" + v.damageHi : "" + v.damageLow;
                range.text = "" + v.range;
                descriptor.text = v.description;
                tname.text = v.name;
            }
            ++x;
        }
    }

    public void onClick(int idx)
    {
        GameState.playerItemIndex = idx;
        generate();
    }



}
