using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBar : MonoBehaviour
{
    public Slider slider;
    public Image fill;
    public TMP_Text mnText;

    public void SetMana(int mana)
    {
        slider.value = mana;
        mnText.text = "MN " + mana + "/" + slider.maxValue;
    }

    public void SetMaxMana(int mana)
    {
        slider.maxValue = mana;
        slider.value = mana;
        mnText.text = "MN " + mana + "/" + mana;
    }
}
