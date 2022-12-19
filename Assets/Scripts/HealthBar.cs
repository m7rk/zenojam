using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public GameObject full;
    public GameObject end;
    public Sprite empty;

    private List<SpriteRenderer> fullbars;
    public void setMax(int count)
    {
        fullbars = new List<SpriteRenderer>();

        var e = Instantiate(end);
        e.transform.SetParent(this.transform);
        e.transform.localPosition = new Vector3(0f, 0f, 0f);

        for(int i = 0; i != count; ++i)
        {
            e = Instantiate(full);
            e.transform.SetParent(this.transform);
            e.transform.localPosition = new Vector3(0.03125f * (i+1), 0f, 0f);
            fullbars.Add(e.GetComponent<SpriteRenderer>());
        }

        e = Instantiate(end);
        e.transform.SetParent(this.transform);
        e.transform.localPosition = new Vector3(0.03125f * (1+count), 0f, 0f);

        this.gameObject.SetActive(false);
    }

    public void hurt()
    {
        fullbars[fullbars.Count - 1].sprite = empty;
        fullbars.RemoveAt(fullbars.Count - 1);
        this.gameObject.SetActive(true);
    }
}
