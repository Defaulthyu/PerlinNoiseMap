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
    private ShopUI shopUI;
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
        shopUI = FindObjectOfType<ShopUI>();
        player = GameObject.FindWithTag("Player");


    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 우클릭(Right Button)인지 확인
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 1. 상점이 열려있다면? -> 판매 목록에 추가
            if (shopUI != null && shopUI.IsShopOpen())
            {
                shopUI.AddToCart(itemType);
                return; // 상점 처리를 했으면 제작 처리는 안 함
            }

            if (craftingPanel != null && craftingPanel.gameObject.activeSelf)
            {
                craftingPanel.AddPlanned(itemType, 1);
            }
        }
    }
}
