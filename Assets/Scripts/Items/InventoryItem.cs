using System;

[Serializable] //[Serializable] will allow the JSON conversion.
public class InventoryItem
{
    public string itemName;
    public string itemDescription;
    //We could save the itemID later for icon display.
    public InventoryItem(string name, string desc)      //Constructor to quickly create items.
    {
        itemName = name;
        itemDescription = desc;
    }
}