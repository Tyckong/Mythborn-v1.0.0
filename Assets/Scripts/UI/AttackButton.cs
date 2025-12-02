using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackButton : MonoBehaviour
{
    public float alphaThreshold = 0.1f;
    public bool isClickable = true;

    private Button btn; //Internal reference to the Button.

    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
        btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(FindAndAttack);
    }

    void FindAndAttack()
    {
        if (!isClickable) return;
        PlayerController player = FindAnyObjectByType<PlayerController>();

        if (player != null)
        {
            player.Attack();
        }
        else
        {
            Debug.LogWarning("AttackButton: No se encontró al PlayerController en la escena.");
        }
    }
    public void clickeableButtonSetter(bool state)
    {
        isClickable = state;
        if (btn != null) btn.interactable = state;
    }

    public bool clickeableButtonGetter()
    {
        return isClickable;
    }
}