using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Sprite[] hpBar;
    public Sprite[] outerPanel;

    public Image hpTop;
    public Image outerRend;

    public readonly float ANIM_SPEED_FRAME = 3f;
    public readonly float ANIM_SPEED_HP = 2f;

    public TMP_Text htp;

    public Image hpBarStretch;

    public readonly float BAR_HEIGHT_MAX = 28;
    public readonly float BAR_MID = 25;
    public readonly float BAR_BOT = 11.5f;

    private float barHDest = 28;
    private float barLenDest = 25;

    private float BAR_ANIM_TIME = 12;

    // Update is called once per frame
    void Update()
    {
        hpTop.sprite = hpBar[(int)((Time.time * ANIM_SPEED_HP)  % hpBar.Length)];
        outerRend.sprite = outerPanel[(int)((Time.time * ANIM_SPEED_FRAME) % outerPanel.Length)];
        var rt = ((RectTransform)hpBarStretch.transform);
        rt.sizeDelta = new Vector2(4, barLenDest);
        rt.anchoredPosition = new Vector2(0, barHDest);
    }

    public void setHP(int hp, int maxHP)
    {
        htp.text = "" + hp;
        // tween later
        barLenDest = BAR_HEIGHT_MAX * (hp/(float)maxHP);
        barHDest = BAR_BOT + (BAR_MID-BAR_BOT) * (hp/(float)maxHP);
    }
}
