using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;                  //This is here to group items with the same ID.

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentArea;           //Area inside the scrollview.
    public GameObject itemButtonPrefab;     //Prefab for an item's button.
    public GameObject confirmationPanel;    //Item-use confirm panel.

    [Header("Confirmation Panel Elements")]
    public TMP_Text confirmationText;       //Text inside the confirm panel.
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public TMP_Text itemQuantityText;

    private string itemPendingToUse;        //Temp variable.

    public void UpdateInventoryDisplay()
    {
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance == null) return;
        List<InventoryItem> rawList = InventoryManager.Instance.inventory;

        var groupedItems = rawList.GroupBy(i => i.itemName);    //Group items by name using LINQ
        foreach (var group in groupedItems)                     //Button for each group.
        {
            GameObject newButton = Instantiate(itemButtonPrefab, contentArea);
            string name = group.Key;
            int count = group.Count();
            string desc = group.First().itemDescription;

            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = $"{name} x{count}";
            }

            Button btn = newButton.GetComponent<Button>();
            btn.onClick.AddListener(() => OpenConfirmationPanel(name, desc, count));
        }
    }

    void OpenConfirmationPanel(string name, string desc, int qty)
    {
        itemPendingToUse = name;            //Remember what item we want to use.
        if (itemNameText) itemNameText.text = name;
        if (itemDescriptionText) itemDescriptionText.text = desc;
        if (itemQuantityText) itemQuantityText.text = "Held: " + qty;
        if (confirmationText) confirmationText.text = $"Are you sure you want to use\n{name}?";
        confirmationPanel.SetActive(true);
    }

    public void OnConfirmYes()
    {
        UseItem(itemPendingToUse);
    }

    public void OnConfirmNo()
    {
        itemPendingToUse = "";
        confirmationPanel.SetActive(false);
    }

    void UseItem(string itemName)
    {
        //Define what each item does. For now, a simple switch would suffice.
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        bool itemUsed = true;

        switch (itemName)
        {
            case "Health Potion":
                player.currentHealth += 20;
                GameManager.Instance.ShowFloatingText("+" + 20, player.transform.position, Color.green);
                if (player.currentHealth > player.characterData.maxHealth) player.currentHealth = player.characterData.maxHealth;
                player.healthBar.SetHealth(player.currentHealth);
                Debug.Log("Health recovered!");
                break;

            case "Mana Potion":
                player.currentMana += 20;
                GameManager.Instance.ShowFloatingText("+" + 20, player.transform.position, Color.blue);
                if (player.currentMana > player.characterData.maxMana) player.currentMana = player.characterData.maxMana;
                player.manaBar.SetMana(player.currentMana);
                break;

            case "Quantum Apple":
                player.currentEnergy += 20;
                GameManager.Instance.ShowFloatingText("+" + 20, player.transform.position, Color.white);
                if (player.currentEnergy > player.characterData.maxEnergy) player.currentEnergy = player.characterData.maxEnergy;
                player.energyBar.SetEnergy(player.currentEnergy);
                break;

            default:
                Debug.Log("This item has no known effect..");
                itemUsed = false;
                break;
        }

        if (itemUsed)       //We delete the item only if it's been used successfully.
        {
            InventoryManager.Instance.RemoveItem(itemName);
            confirmationPanel.SetActive(false);
            GameManager.Instance.CombatMenu();
            GameManager.Instance.EndPlayerTurn();
        }
    }
}