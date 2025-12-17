using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    public Dictionary<ItemType, int> items = new();
    InventoryUI inventoryUI;
    public int gold = 0;

    private void Start()
    {
        inventoryUI = FindObjectOfType<InventoryUI>();
    }
    public void AddGold(int amount)
    {
        gold += amount;
        if (inventoryUI != null) inventoryUI.UpdateGoldText(gold);
        Debug.Log($"[Money] ÇöÀç °ñµå: {gold}");
    }

    public bool UseGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            if (inventoryUI != null) inventoryUI.UpdateGoldText(gold);
            return true;
        }
        return false;
    }

    public int GetCount(ItemType id)
    {
        items.TryGetValue(id, out var count);
        return count;
    }

    // void -> bool·Î º¯°æ
    public bool Add(ItemType type, int count = 1)
    {

        if (!items.ContainsKey(type)) items[type] = 0;
        items[type] += count;

        Debug.Log($"[Inventory] +{count} {type} (ÃÑ {items[type]})");

        if (inventoryUI != null) inventoryUI.UpdateInventoryUI(this);

        return true;
    }

    public bool Consume(ItemType type, int count = 1)
    {
        if (!items.TryGetValue(type, out var have) || have < count) return false;
        items[type] = have - count;
        Debug.Log($"[Inventory] -{count} {type} (ÃÑ {items[type]})");
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
