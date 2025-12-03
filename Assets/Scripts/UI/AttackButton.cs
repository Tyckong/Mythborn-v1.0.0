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
        btn = GetComponent<Button>();

        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(FindAndAttack);
        }

        try
        {
            var img = GetComponent<Image>();
            if (img != null)
            {
                img.alphaHitTestMinimumThreshold = alphaThreshold;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Couldn't configure button's transparency: " + e.Message);
        }
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