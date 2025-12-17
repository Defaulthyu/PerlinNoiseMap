using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerHarvester : MonoBehaviour
{
    public float rayDistance = 5f;          //채집 가능 거리
    public LayerMask hitMask = ~0;          //가능한 레이어 전부다
    public int toolDamage = 1;
    public float hitColldown = 0.15f;       //연타 간격
    private float _nextHitTime;
    private Camera _cam;
    public Inventory inventory;             //플레이어 인벤
    InventoryUI inventoryUI;
    public GameObject selectedBlock;
    private void Awake()
    {
        _cam = Camera.main;
        if (inventory == null) inventory = gameObject.AddComponent<Inventory>();
        inventoryUI = FindObjectOfType<InventoryUI>();
    }
    private void Update()
    {
        if (Cursor.visible) return;

        // 현재 선택된 아이템 가져오기
        ItemType currentItem = ItemType.None;
        if (inventoryUI.selectedIndex >= 0)
        {
            currentItem = inventoryUI.GetInventorySlot();
        }

        // ▼▼▼ 로직 분기: 도구를 들었거나 맨손이면 [채굴], 블록을 들었으면 [건설] ▼▼▼
        if (IsTool(currentItem) || currentItem == ItemType.None)
        {
            HandleMining(currentItem); // 채굴 모드
        }
        else
        {
            HandleBuilding(currentItem); // 건설 모드
        }
    }

    void HandleMining(ItemType item)
    {
        // 건설용 미리보기 숨기기
        selectedBlock.transform.localScale = Vector3.zero;

        // 마우스 클릭 시 채굴
        if (Input.GetMouseButton(0) && Time.time >= _nextHitTime)
        {
            _nextHitTime = Time.time + hitColldown;

            // 도구 데미지 계산
            int damage = GetDamage(item);
            int myTier = GetToolTier(item);

            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1f);
            if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                // 1. NPC 상호작용 (우선순위)
                if (hit.collider.TryGetComponent(out ShopNPC shop))
                {
                    shop.Interact(inventory);
                    return;
                }

                // 2. 블록 채굴
                var block = hit.collider.GetComponent<Block>();
                if (block != null)
                {
                    if (myTier >= block.minTier)
                    {
                        Debug.Log("채굴!");
                        block.Hit(damage, inventory);
                    }
                    else
                    {
                        Debug.Log($"등급 부족! 필요: {block.minTier}, 현재: {myTier}");
                    }
                }
            }
        }
    }

    // [건설 모드] - 기존 코드 유지
    void HandleBuilding(ItemType item)
    {
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // 미리보기 표시
        if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            Vector3Int placePos = AdjacentCellOnHitFace(hit);
            selectedBlock.transform.localScale = Vector3.one;
            // 부드러운 이동 (선택사항)
            selectedBlock.transform.position = Vector3.Lerp(selectedBlock.transform.position, placePos, Time.deltaTime * 15f);
            selectedBlock.transform.rotation = Quaternion.identity;

            // 클릭 시 설치
            if (Input.GetMouseButtonDown(0))
            {
                if (inventory.Consume(item, 1))
                {
                    FindObjectOfType<NoiseVoxelMap>().PlaceTile(placePos, item);
                }
            }
        }
        else
        {
            selectedBlock.transform.localScale = Vector3.zero;
        }
    }

    // 도구인지 판별하는 함수
    bool IsTool(ItemType type)
    {
        return type == ItemType.GoldPickaxe ||
               type == ItemType.StonePickaxe ||
               type == ItemType.IronPickaxe ||
               type == ItemType.DiamondPickaxe;
    }

    // 도구별 데미지 설정 함수
    int GetDamage(ItemType type)
    {
        switch (type)
        {
            case ItemType.StonePickaxe: return 2;   // 돌 곡괭이
            case ItemType.IronPickaxe: return 3;    // 철 곡괭이
            case ItemType.GoldPickaxe: return 5;    // 금 곡괭이
            case ItemType.DiamondPickaxe: return 30;// 다이아 곡괭이
            default: return 1;                      // 맨손
        }
    }
    int GetToolTier(ItemType type)
    {
        switch (type)
        {
            case ItemType.DiamondPickaxe: return 4;
            case ItemType.GoldPickaxe: return 3;
            case ItemType.IronPickaxe: return 2;
            case ItemType.StonePickaxe: return 1;
            default: return 0; // 맨손 혹은 다른 아이템
        }
    }

    static Vector3Int AdjacentCellOnHitFace(in RaycastHit hit)
    {
        Vector3 baseCenter = hit.collider.transform.position;
        Vector3 adjCenter = baseCenter + hit.normal;
        return Vector3Int.RoundToInt(adjCenter);
    }

}
