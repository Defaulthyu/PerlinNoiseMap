using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Dictionary<ItemType, int> items = new();
    InventoryUI inventoryUI;

    private void Start()
    {
        inventoryUI = FindObjectOfType<InventoryUI>();
    }

    public int GetCount(ItemType id)
    {
        items.TryGetValue(id, out var count);
        return count;
    }

    public void Add(ItemType type, int count = 1)
    {
        if (!items.ContainsKey(type)) items[type] = 0;
        items[type] += count;
        Debug.Log($"[Inventory] +{count} {type} (รั {items[type]})");
        inventoryUI.UpdateInventoryUI(this);
    }

    public bool Consume(ItemType type, int count = 1)
    {
        if (!items.TryGetValue(type, out var have) || have < count) return false;
        items[type] = have - count;
        Debug.Log($"[Inventory] -{count} {type} (รั {items[type]})");
        if (items[type] == 0)
        {
            items.Remove(type);
            inventoryUI.selectedIndex = -1;
            inventoryUI.ResetSelection();
        }
        inventoryUI.UpdateInventoryUI(this);
        return true;
    }
}
