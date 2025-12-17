using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    // 아이템별 판매 가격 (플레이어가 팔 때 받는 돈)
    public Dictionary<ItemType, int> sellPrices = new Dictionary<ItemType, int>()
    {
        { ItemType.Stone, 2 },
        { ItemType.Coal, 10 },
        { ItemType.Gold, 50 },
        { ItemType.Diamond, 200 }
    };

    // 아이템별 구매 가격 (플레이어가 살 때 내는 돈)
    public Dictionary<ItemType, int> buyPrices = new Dictionary<ItemType, int>()
    {
        { ItemType.StonePickaxe, 100 }, // 예: 강화 곡괭이
        { ItemType.IronPickaxe, 200 },
        { ItemType.GoldPickaxe, 400 },
        { ItemType.DiamondPickaxe, 500 }
    };

    public ShopUI shopUI; // UI 연결

    private void Start()
    {
        // UI가 없으면 찾아서 연결
        if (shopUI == null) shopUI = FindObjectOfType<ShopUI>();
    }

    // 플레이어가 클릭했을 때 호출됨
    public void Interact(Inventory playerInv)
    {
        Debug.Log("상점 주인: 어서오게!");
        shopUI.OpenShop(this, playerInv);
        Cursor.lockState = CursorLockMode.None;
    }
}