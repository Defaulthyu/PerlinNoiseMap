using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 텍스트 표시용
using System.Text; // StringBuilder용

public class ShopUI : MonoBehaviour
{
    public Inventory playerInventory;
    public ShopNPC currentNPC; // 현재 열린 상점 NPC (가격표 확인용)

    [Header("UI References")]
    public GameObject root;         // 상점 UI 전체 부모
    public TMP_Text cartListText;   // 판매 목록 텍스트
    public TMP_Text totalPriceText; // "총 합계: 0 G" 표시

    // 판매 대기 목록 (아이템타입, 개수)
    private Dictionary<ItemType, int> sellingCart = new Dictionary<ItemType, int>();

    private bool isOpen = false;

    // 상점 열기
    public void OpenShop(ShopNPC npc)
    {
        currentNPC = npc;
        sellingCart.Clear(); // 열 때마다 장바구니 초기화
        UpdateCartUI();

        isOpen = true;
        root.SetActive(true);

        // 커서 보이기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // 상점 닫기
    public void CloseShop()
    {
        sellingCart.Clear();
        isOpen = false;
        root.SetActive(false);

        // 커서 숨기기 (상황에 따라 다름)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void AddToCart(ItemType type)
    {
        if (!isOpen || currentNPC == null) return;

        // 1. 이 NPC가 매입하는 물건인지(가격이 있는지) 확인
        if (!currentNPC.sellPrices.ContainsKey(type))
        {
            Debug.Log("이 상점에서는 취급하지 않는 물건입니다.");
            return;
        }

        // 2. 인벤토리에 있는 개수보다 더 많이 담을 순 없음
        int currentInInv = playerInventory.GetCount(type);
        int currentInCart = sellingCart.ContainsKey(type) ? sellingCart[type] : 0;

        if (currentInCart < currentInInv)
        {
            if (!sellingCart.ContainsKey(type)) sellingCart[type] = 0;
            sellingCart[type]++; // 1개 추가

            UpdateCartUI(); // UI 갱신
        }
        else
        {
            Debug.Log("더 이상 팔 물건이 없습니다.");
        }
    }

    // 장바구니 비우기 버튼용
    public void ClearCart()
    {
        sellingCart.Clear();
        UpdateCartUI();
    }

    public void ConfirmSell()
    {
        if (sellingCart.Count == 0) return;

        int totalEarned = 0;

        foreach (var item in sellingCart)
        {
            ItemType type = item.Key;
            int count = item.Value;
            int price = currentNPC.sellPrices[type];

            if (playerInventory.Consume(type, count))
            {
                totalEarned += (price * count);
            }
        }

        // 2. 돈 지급
        if (totalEarned > 0)
        {
            playerInventory.AddGold(totalEarned);
            Debug.Log($"판매 완료! 총수익: {totalEarned} G");
        }

        // 3. 장바구니 초기화
        ClearCart();
    }

    // UI 그려주는 함수
    void UpdateCartUI()
    {
        if (cartListText == null) return;

        StringBuilder sb = new StringBuilder();
        int grandTotal = 0;

        if (sellingCart.Count == 0)
        {
            sb.AppendLine("판매할 물건을 우클릭하세요.");
        }
        else
        {
            foreach (var item in sellingCart)
            {
                if (currentNPC.sellPrices.TryGetValue(item.Key, out int price))
                {
                    int sum = price * item.Value;
                    grandTotal += sum;

                    sb.AppendLine($"{item.Key} x {item.Value} ({sum} G)");
                }
            }
        }

        cartListText.text = sb.ToString();

        if (totalPriceText != null)
            totalPriceText.text = $"총 합계: {grandTotal} G";
    }

    public bool IsShopOpen() => isOpen;
}