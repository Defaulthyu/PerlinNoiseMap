using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // <- 이거 꼭 맨 위에 있어야 해요!

public class InventoryUI : MonoBehaviour
{
    [System.Serializable] // 인스펙터에 보이게 하는 속성
    public struct ItemIconData
    {
        public ItemType type;
        public Sprite icon;
    }

    [Header("Item Icons")]
    public List<ItemIconData> itemIcons = new List<ItemIconData>();

    public List<Transform> Slot = new List<Transform>();
    public GameObject SlotItem;
    List<GameObject> items = new List<GameObject>();

    [Header("Gold UI")]
    public TextMeshProUGUI goldText;

    public int selectedIndex = -1;

    Sprite GetSpriteByType(ItemType type)
    {
        // 리스트를 뒤져서 type이 같은 녀석의 아이콘을 반환
        foreach (var data in itemIcons)
        {
            if (data.type == type) return data.icon;
        }
        return null; // 없으면 null
    }

    public void UpdateInventoryUI(Inventory myInven)
    {
        // 슬롯 초기화
        foreach (var slotItems in items)
        {
            Destroy(slotItems);
        }
        items.Clear();

        int index = 0;
        foreach (var item in myInven.items)
        {
            if(index >= Slot.Count)
            {
                Debug.LogWarning("슬롯보다 아이템이 많습니다");
                break;
            }
            var go = Instantiate(SlotItem, Slot[index].transform);
            go.transform.localPosition = Vector3.zero;
            SlotItemPrefab sItem = go.GetComponent<SlotItemPrefab>();
            items.Add(go);

            Sprite icon = GetSpriteByType(item.Key);

            if (icon != null)
            {
                sItem.ItemSetting(icon, "x" + item.Value.ToString(), item.Key);
            }
            else
            {
                Debug.LogWarning($"[InventoryUI] {item.Key}의 이미지가 할당되지 않았습니다!");
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
        if (selectedIndex == idx)
        {
            selectedIndex = -1;
        }
        else
        {
            selectedIndex = idx;
        }

        // 3. 화면 갱신 (색상 바꾸기)
        UpdateSelectionVisuals();
    }

    public void ResetSelection()
    {
        selectedIndex = -1;
        UpdateSelectionVisuals();
    }
    public void UpdateSelectionVisuals()
    {
        for (int i = 0; i < Slot.Count; i++)
        {
            Image slotBg = Slot[i].GetComponent<Image>();

            if (slotBg != null)
            {
                if (i == selectedIndex)
                {
                    slotBg.color = Color.yellow;
                }
                else
                {
                    slotBg.color = Color.white;
                }
            }
        }
    }

    public ItemType GetInventorySlot()
    {
        if (selectedIndex < 0 || selectedIndex >= items.Count)
        {
            return ItemType.None;
        }

        return items[selectedIndex].GetComponent<SlotItemPrefab>().itemType;
    }

    public void UpdateGoldText(int gold)
    {
        // 텍스트 오브젝트가 연결되어 있다면 내용을 바꿈
        if (goldText != null)
        {
            goldText.text = gold.ToString("N0") + " G"; // N0는 1,000 처럼 쉼표를 찍어줍니다
        }
    }
}
