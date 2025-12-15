using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Unity.Collections.AllocatorManager;

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
    private WorldGenerator worldGenerator;
    public float hitCooldown = 0.05f;
    private void Awake()
    {
        _cam = Camera.main;
        if (inventory == null) inventory = gameObject.AddComponent<Inventory>();
        inventoryUI = FindObjectOfType<InventoryUI>();
        worldGenerator = FindAnyObjectByType<WorldGenerator>();
    }
    private void Update()
    {
        // 0. 필수 참조가 없으면 실행하지 않음 (에러 방지)
        if (inventoryUI == null || worldGenerator == null) return;

        // 화면 정중앙을 향하는 레이 생성
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        bool hasHit = Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore);

        // ====================================================
        // 상황 1: 맨손일 때 (채광 모드)
        // ====================================================
        if (inventoryUI.selectedIndex < 0)
        {
            // 설치 미리보기 끄기
            if (selectedBlock != null) selectedBlock.SetActive(false);

            // 좌클릭 + 쿨타임 체크 + 무언가 바라보고 있음
            if (Input.GetMouseButton(0) && Time.time >= _nextHitTime && hasHit)
            {
                var block = hit.collider.GetComponent<Block>();
                if (block != null)
                {
                    _nextHitTime = Time.time + hitCooldown; // 쿨타임 갱신
                    block.Hit(toolDamage, inventory);       // 블록 타격

                    // (선택사항) 타격 효과음이나 파티클 재생 로직 추가 가능
                }
            }
        }
        // ====================================================
        // 상황 2: 아이템을 들고 있을 때 (설치 또는 도구 사용)
        // ====================================================
        else
        {
            // 현재 선택된 아이템의 타입 가져오기
            ItemType selectedItem = inventoryUI.GetInventorySlot();
            bool isTool = IsTool(selectedItem); // 도구인지 확인 (아래 함수 참고)

            // 2-A. 블록을 들고 있고, 설치할 곳(hit)이 있을 때
            if (!isTool && hasHit)
            {
                // 설치될 좌표 계산
                Vector3Int placePos = AdjacentCellOnHitFace(hit);

                // 미리보기 블록 활성화 및 위치 이동
                if (selectedBlock != null)
                {
                    selectedBlock.SetActive(true);
                    selectedBlock.transform.position = placePos;
                    selectedBlock.transform.rotation = Quaternion.identity;
                }

                // 좌클릭 시 설치 시도
                if (Input.GetMouseButtonDown(0))
                {
                    // 플레이어와 설치 위치가 너무 가까우면 설치 불가 (끼임 방지)
                    if (Vector3.Distance(transform.position, placePos) > 1.5f)
                    {
                        // 인벤토리에서 아이템 1개 소모 성공 시
                        if (inventory.Consume(selectedItem, 1))
                        {
                            // WorldGenerator에게 설치 요청
                            // (ItemType과 BlockType의 순서가 같다고 가정하고 형변환)
                            worldGenerator.PlaceBlock(placePos, (BlockType)selectedItem);
                        }
                    }
                }
            }
            // 2-B. 도구를 들고 있거나, 허공을 바라볼 때
            else
            {
                // 미리보기 끄기
                if (selectedBlock != null) selectedBlock.SetActive(false);

                // 도구 사용 액션 (공격 등)
                if (isTool && Input.GetMouseButtonDown(0))
                {
                    // 여기에 도구 사용 로직 추가 (예: 애니메이션 트리거)
                    Debug.Log($"{selectedItem}을(를) 휘둘렀습니다!");

                    // 도구로도 채광을 하고 싶다면 여기에 채광 로직 추가 가능
                    if (hasHit)
                    {
                        var block = hit.collider.GetComponent<Block>();
                        if (block != null) block.Hit(toolDamage, inventory);
                    }
                }
            }
        }
    }

    bool IsTool(ItemType item)
    {
        // ItemType Enum에서 정의한 도구들인지 확인
        return item == ItemType.Axe || item == ItemType.Pickaxe || item == ItemType.Sword;
    }

    static Vector3Int AdjacentCellOnHitFace(in RaycastHit hit)
    {
        Vector3 baseCenter = hit.collider.transform.position;
        Vector3 adjCenter = baseCenter + hit.normal;
        return Vector3Int.RoundToInt(adjCenter);
    }

}
