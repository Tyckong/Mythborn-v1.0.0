using UnityEngine;

public class LootItem : MonoBehaviour
{
    [Header("Item Data")]
    public string itemName;
    [TextArea] public string itemDescription;

    public void SetItemData(string name, string desc)
    {
        itemName = name;
        itemDescription = desc;
    }
}