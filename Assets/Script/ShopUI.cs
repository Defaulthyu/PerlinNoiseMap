using UnityEngine;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public GameObject shopPanel; // 상점 전체 패널 (UI)
    public TextMeshProUGUI goldText; // 현재 소지금 표시 텍스트

    private ShopNPC currentNPC;
    private Inventory playerInventory;

    private void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<Inventory>();
        }

        shopPanel.SetActive(false); // 상점 창 숨기기

        if (playerInventory != null)
        {
            UpdateGoldText(playerInventory.gold);
        }
    }

    // 상점 열기
    public void OpenShop(ShopNPC npc, Inventory inventory)
    {
        currentNPC = npc;
        playerInventory = inventory;
        shopPanel.SetActive(true);
        UpdateGoldText(playerInventory.gold);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // 상점 닫기 (X 버튼용)
    public void CloseShop()
    {
        shopPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SellItem(string itemTypeName) // 버튼 OnClick에서는 Enum을 바로 못넣어서 string으로 받음
    {
        // string을 Enum으로 변환
        if (System.Enum.TryParse(itemTypeName, out ItemType type))
        {
            // 인벤토리에 아이템이 있는지 확인
            if (playerInventory.Consume(type, 1))
            {
                // 가격 확인 후 돈 지급
                if (currentNPC.sellPrices.TryGetValue(type, out int price))
                {
                    playerInventory.AddGold(price);
                    Debug.Log($"{type} 판매 완료! +{price} Gold");
                }
                else
                {
                    // 가격표에 없으면 공짜로 사라지니 다시 돌려주거나 예외처리 (여기선 간단히 1골드)
                    playerInventory.AddGold(1);
                }
            }
            else
            {
                Debug.Log("팔 아이템이 없습니다.");
            }
        }
    }

    // [기능 2] 도구 사기 버튼에 연결할 함수
    public void BuyItem(string itemTypeName)
    {
        if (System.Enum.TryParse(itemTypeName, out ItemType type))
        {
            if (currentNPC.buyPrices.TryGetValue(type, out int price))
            {
                if (playerInventory.UseGold(price))
                {
                    playerInventory.Add(type, 1);
                    Debug.Log($"{type} 구매 완료! -{price} Gold");
                }
                else
                {
                    Debug.Log("돈이 부족합니다.");
                }
            }
        }
    }

    public void UpdateGoldText(int gold)
    {
        if (goldText != null) goldText.text = $"{gold} G";
    }
}