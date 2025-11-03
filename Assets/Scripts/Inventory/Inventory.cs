using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 16;
    public List<InventoryItem> items = new List<InventoryItem>();

    public bool AddItem(InventoryItem item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventory is full");
            return false;
        }
        items.Add(item);
        return true;
    }

    public void RemoveItem(InventoryItem item)
    {
        items.Remove(item);
    }
}
