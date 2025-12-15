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

    public void Add(ItemType type, int count = 1)
    {
        if (!items.ContainsKey(type)) items[type] = 0;
        items[type] += count;

        Debug.Log($"[Inventory] +{count} {type} (총 {items[type]})");
        inventoryUI.UpdateInventoryUI(this);
    }

    public bool Consume(ItemType type, int count = 1)
    {
        if (!items.TryGetValue(type, out var have) || have < count) return false;

        items[type] = have - count;
        Debug.Log($"[Inventory] -{count} {type} (총 {items[type]})");

        if (items[type] == 0)
        {
            items.Remove(type);

            // 아이템이 0개가 되어 사라졌을 때 선택 해제 로직
            // (만약 현재 쥐고 있는 아이템이 사라졌다면)
            // 간단하게 구현하기 위해 일단 UI 업데이트에 맡김
            if (inventoryUI.GetInventorySlot() == type)
            {
                inventoryUI.selectedIndex = -1;
                inventoryUI.ResetSelection();
            }
        }

        inventoryUI.UpdateInventoryUI(this);
        return true;
    }
}