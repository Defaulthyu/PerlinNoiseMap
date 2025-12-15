using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Sprite dirtSprite;
    public Sprite grassSprite;
    public Sprite waterSprite;
    public Sprite stoneSprite;
    public Sprite coalSprite;
    public Sprite ironSprite;
    public Sprite goldSprite;
    public Sprite diamondSprite;

    public List<Transform> Slot = new List<Transform>();
    public GameObject SlotItem;
    List<GameObject> items = new List<GameObject>();

    public int selectedIndex = -1;
    public void UpdateInventoryUI(Inventory myInven)
    {
        // 모든 슬롯 초기화
        foreach (var slotItems in items)
        {
            Destroy(slotItems);
        }
        items.Clear();

        // Dictionary에 있는 아이템들을 순서대로 표시
        int index = 0;
        foreach (var item in myInven.items)
        {
            // 슬롯 개수를 초과하면 표시 중단
            if (index >= Slot.Count) break;

            var go = Instantiate(SlotItem, Slot[index].transform);
            go.transform.localPosition = Vector3.zero;

            SlotItemPrefab sItem = go.GetComponent<SlotItemPrefab>();
            items.Add(go);

            Sprite targetSprite = null;
            switch (item.Key)
            {
                case ItemType.Dirt: targetSprite = dirtSprite; break;
                case ItemType.Grass: targetSprite = grassSprite; break;
                case ItemType.Water: targetSprite = waterSprite; break;
                case ItemType.Stone: targetSprite = stoneSprite; break;
                case ItemType.Coal: targetSprite = coalSprite; break;
                case ItemType.Iron: targetSprite = ironSprite; break;
                case ItemType.Gold: targetSprite = goldSprite; break;
                case ItemType.Diamond: targetSprite = diamondSprite; break;

                default: targetSprite = dirtSprite; break; // 기본값
            }

            sItem.ItemSetting(targetSprite, "x" + item.Value.ToString(), item.Key);
            index++;
        }
    }
    private void Update()
    {
        for (int i = 0; i < Mathf.Min(9, Slot.Count); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetSelectedIndex(i);
            }
        }
    }

    public void SetSelectedIndex(int idx)
    {
        ResetSelection();
        if (selectedIndex == idx)
        {
            selectedIndex = -1; // 이미 선택된거 누르면 해제
        }
        else
        {
            if (idx >= items.Count)
            {
                selectedIndex = -1;
            }
            else
            {
                SetSelection(idx);
                selectedIndex = idx;
            }
        }
    }

    public void ResetSelection()
    {
        foreach (var slot in Slot)
        {
            slot.GetComponent<Image>().color = Color.white;
        }
    }

    void SetSelection(int _idx)
    {
        Slot[_idx].GetComponent<Image>().color = Color.yellow;
    }

    public ItemType GetInventorySlot()
    {
        // 선택된 게 없거나 범위 밖이면 Air 반환 (Air는 ItemType에 0번으로 추가해주면 좋음, 없으면 Dirt로)
        if (selectedIndex < 0 || selectedIndex >= items.Count) return ItemType.Dirt;

        return items[selectedIndex].GetComponent<SlotItemPrefab>().itemType;
    }
}
