using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Sprite[] hpBar;
    public Sprite[] outerPanel;

    public Image hpRend;
    public Image outerRend;

    public readonly float ANIM_SPEED_FRAME = 3f;
    public readonly float ANIM_SPEED_HP = 2f;

    public TMP_Text htp;

    // Update is called once per frame
    void Update()
    {
        hpRend.sprite = hpBar[(int)((Time.time * ANIM_SPEED_HP)  % hpBar.Length)];
        outerRend.sprite = outerPanel[(int)((Time.time * ANIM_SPEED_FRAME) % outerPanel.Length)];
    }

    public void setHP(int hp)
    {
        htp.text = ""+hp;
    }
}
