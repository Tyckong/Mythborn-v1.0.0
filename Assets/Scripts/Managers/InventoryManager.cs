using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;                    //For reeading and writing files.

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Data")]
    public List<InventoryItem> inventory = new List<InventoryItem>();

    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //Inventory remains between loads.
            saveFilePath = Application.persistentDataPath + "/player_inventory.json";
            LoadInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(string name, string description)
    {
        InventoryItem newItem = new InventoryItem(name, description);
        inventory.Add(newItem);
        Debug.Log("Item added: " + name);
        SaveInventory();            //We autosave everytime we add something to our inventory.
    }

    public void RemoveItem(string itemName)
    {
        InventoryItem itemToRemove = inventory.Find(x => x.itemName == itemName);
        if (itemToRemove != null)
        {
            inventory.Remove(itemToRemove);
            SaveInventory();
            Debug.Log("Item consumed: " + itemName);
        }
    }

    public void SaveInventory()
    {
        InventoryData dataToSave = new InventoryData();     //Wrapper for the inventory list.
        dataToSave.savedItems = inventory;
        string json = JsonUtility.ToJson(dataToSave, true);
        //Might want to encrypt the json here.
        File.WriteAllText(saveFilePath, json);

        Debug.Log("Inventory saved in: " + saveFilePath);
    }

    public void LoadInventory()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            InventoryData dataLoaded = JsonUtility.FromJson<InventoryData>(json);
            inventory = dataLoaded.savedItems;

            Debug.Log("Inventory loaded. Items: " + inventory.Count);
        }
        else
        {
            Debug.Log("Save file not found. Creating a new one.");
        }
    }

    //Add items with the P-key, this is just for debugging, Tomás, please remember to remove this!!!
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddItem("Health Potion", "Heals the player for 10 HP! Yummies!");
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Deleting inventory...");
            inventory.Clear();
            SaveInventory();
        }
#endif
    }
}

[Serializable]
public class InventoryData
{
    public List<InventoryItem> savedItems;
}