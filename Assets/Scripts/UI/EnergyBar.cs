using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyBar : MonoBehaviour
{
    public Slider slider;
    public Image fill;
    public TMP_Text mnText;

    public void SetEnergy(int energy)
    {
        slider.value = energy;
        mnText.text = "EN " + energy + "/" + slider.maxValue;
    }

    public void SetMaxEnergy(int energy)
    {
        slider.maxValue = energy;
        slider.value = energy;
        mnText.text = "EN " + energy + "/" + energy;
    }
}
