using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Sprite dirtSprite;
    public Sprite grassSprite;
    public Sprite waterSprite;
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
        //아이템 탐색

        //  Dictionary에 있는 아이템들을 순서대로 표시
        int index = 0;
        foreach (var item in myInven.items)
        {
            var go = Instantiate(SlotItem, Slot[index].transform);
            go.transform.localPosition = Vector3.zero;
            SlotItemPrefab sItem = go.GetComponent<SlotItemPrefab>();
            items.Add(go);

            switch (item.Key)
            {
                case BlockType.Dirt:
                    sItem.ItemSetting(dirtSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case BlockType.Grass:
                    sItem.ItemSetting(grassSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case BlockType.Water:
                    sItem.ItemSetting(waterSprite, "x" + item.Value.ToString(), item.Key);
                    break;
            }
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
            selectedIndex = -1;
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

    public BlockType GetInventorySlot()
    {
        return items[selectedIndex].GetComponent<SlotItemPrefab>().blockType;
    }
}
