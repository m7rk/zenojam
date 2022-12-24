using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicAnimsets : MonoBehaviour
{
    public Sprite[] purple;
    public Sprite[] green;
    public Sprite[] blue;
    public Sprite[] yellow;
    public Sprite[] orange;


    public Sprite[] animsetForSpell(string aname)
    {
        switch (aname)
        {
            case "Blaze":
                return blue;
            case "Combust":
                return orange;
            case "Firewall":
                return green;
            case "Inferno":
                return green;
            case "Singe":
                return purple;
        }
        return new Sprite[] { };
    }
}
