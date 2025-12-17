using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotItemPrefab : MonoBehaviour, IPointerClickHandler
{
    public Image itemImage;
    public TextMeshProUGUI itemText;
    public ItemType itemType;
    public CraftingPanel craftingPanel;

    private GameObject player;
    public void ItemSetting(Sprite itemSprite, string txt, ItemType type)
    {
        itemImage.sprite = itemSprite;
        itemType = type;
        itemText.text = txt;
    }

    void Awake()
    {
        if (!craftingPanel)
            craftingPanel = FindObjectOfType<CraftingPanel>(true);
        player = GameObject.FindWithTag("Player");


    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (!craftingPanel) return;

        craftingPanel.AddPlanned(itemType, 1);
    }
}
