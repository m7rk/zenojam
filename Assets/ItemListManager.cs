using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemListManager : MonoBehaviour
{
    public GameObject clickableItemBase;
    public List<GameObject> buttons;

    // Start is called before the first frame update
    public void generate(List<GameItem> items)
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
        foreach(var v in items)
        {
            var it = Instantiate(clickableItemBase);
            it.transform.SetParent(this.transform);
            it.GetComponent<Image>().sprite = v.image;
            it.transform.localScale = Vector3.one;
            it.transform.localPosition = new Vector3(48*x, 0, 0);
            buttons.Add(it);
            ++x;
        }
    }

}
