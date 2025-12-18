using UnityEngine;

public class CheatItem : MonoBehaviour
{
    [Header("줄 아이템 설정")]
    public ItemType itemToGive;
    public int count = 1;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그가 "Player"인지 확인
        if (other.CompareTag("Player"))
        {
            Inventory inventory = other.GetComponent<Inventory>();

            if (inventory != null)
            {
                // 아이템 지급
                inventory.Add(itemToGive, count);
                Debug.Log($"[테스트] {itemToGive} 획득!");

                Destroy(gameObject);
            }
        }
    }
}