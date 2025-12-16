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
        if (inventoryUI.selectedIndex < 0)
        {
            selectedBlock.transform.localScale = Vector3.zero;
            if (Input.GetMouseButton(0) && Time.time >= _nextHitTime)
            {
                _nextHitTime = Time.time + hitColldown;

                Ray rayDebug = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));  //화면 중앙
                if (Physics.Raycast(rayDebug, out var hitDebug, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
                {
                    var block = hitDebug.collider.GetComponent<Block>();
                    if (block != null)
                    {
                        block.Hit(toolDamage, inventory);
                    }
                }

            }
        }
        else
        {
            //선택한 idx가 0 이상이면 설치모드
            Ray dray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));  //화면 중앙
            if (Physics.Raycast(dray, out var dhit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                Vector3Int placePos = AdjacentCellOnHitFace(dhit);
                selectedBlock.transform.localScale = Vector3.one;
                selectedBlock.transform.position = placePos;
                selectedBlock.transform.rotation = Quaternion.identity;
            }
            else
            {
                selectedBlock.transform.localScale = Vector3.zero;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

                if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
                {
                    Vector3Int placePos = AdjacentCellOnHitFace(hit);

                    ItemType selected = inventoryUI.GetInventorySlot();
                    if (inventory.Consume(selected, 1))
                    {
                        FindObjectOfType<NoiseVoxelMap>().PlaceTile(placePos, selected);
                    }
                }
            }
        }

    }
    static Vector3Int AdjacentCellOnHitFace(in RaycastHit hit)
    {
        Vector3 baseCenter = hit.collider.transform.position;
        Vector3 adjCenter = baseCenter + hit.normal;
        return Vector3Int.RoundToInt(adjCenter);
    }

}
