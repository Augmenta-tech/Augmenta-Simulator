using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutController : MonoBehaviour
{
    public TMPro.TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        text.text = "<b><size=18>ABOUT</size></b>\n \nAugmenta Simulator v"+ Application.version + "\n<color=#0088ffff><u><link=\"AugmentaTech\">www.augmenta-tech.com</link></u></color>";
    }
}
