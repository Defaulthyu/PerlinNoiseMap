using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseVoxelMap : MonoBehaviour
{
    [Header("Basic Blocks")]
    public GameObject grassPrefab;
    public GameObject dirtPrefab;
    public GameObject waterPrefab;
    public GameObject stonePrefab; // 새로 추가: 돌

    [Header("Ores")]
    public GameObject coalPrefab;    // 석탄
    public GameObject ironPrefab;
    public GameObject goldPrefab;    // 금
    public GameObject diamondPrefab; // 다이아

    [Header("Map Settings")]
    public int width = 20;
    public int depth = 20;
    public int waterLevel = 5;       // 변수명 명확하게 변경 (water -> waterLevel)
    public int maxHeight = 16;
    [SerializeField] float noiseScale = 20f;

    [Header("Ore Rarity (0.0 ~ 1.0)")]
    public float coalChance = 0.08f;   // 8%
    public float ironChance = 0.05f;   //5%
    public float goldChance = 0.04f;   // 4%
    public float diamondChance = 0.01f;// 1%

    public int currentStage = 1;        //1: 기본 2: 강한 돌 바닥 3: 다이아 바닥

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        float offsetX = Random.Range(-9999f, 9999f);
        float offsetZ = Random.Range(-9999f, 9999f);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 1. 지형 높이 결정 (Height Map)
                float nx = (x + offsetX) / noiseScale;
                float nz = (z + offsetZ) / noiseScale;
                float noise = Mathf.PerlinNoise(nx, nz);
                int h = Mathf.FloorToInt(noise * maxHeight);
                if (h <= 0) h = 1;

                // 2. 바닥부터 높이 h까지 블록 쌓기
                for (int y = 0; y <= h; y++)
                {
                    if (y == 0)
                    {
                        CreateStageFloor(x, y, z);
                        continue; // 바닥을 깔았으면 다음 높이로 넘어감 (DetermineBlockToPlace 실행 X)
                    }

                    DetermineBlockToPlace(x, y, z, h);
                }

                // 3. 물 채우기
                for (int y = h + 1; y < waterLevel; y++)
                {
                    PlaceBlock(x, y, z, waterPrefab, ItemType.Water, false);
                }
            }
        }
    }

    // 어떤 블록을 놓을지 결정하는 로직
    void DetermineBlockToPlace(int x, int y, int z, int surfaceHeight)
    {
        // 1. 최상단: 잔디
        if (y == surfaceHeight)
        {
            // 물 높이보다 낮으면 모래(혹은 흙)가 자연스럽지만 일단 잔디로 유지
            if (y < waterLevel)
                PlaceBlock(x, y, z, dirtPrefab, ItemType.Dirt, true); // 물 밑은 흙
            else
                PlaceBlock(x, y, z, grassPrefab, ItemType.Grass, true);
            return;
        }

        // 2. 표면 바로 아래 (3칸 정도): 흙
        if (y > surfaceHeight - 3)
        {
            PlaceBlock(x, y, z, dirtPrefab, ItemType.Dirt, true);
            return;
        }

        // 3. 그 아래 깊은 곳: 돌 + 광물
        // 확률적으로 광물 생성 (깊을수록 좋은 광물이 나오게 하려면 y값을 확률 계산에 넣으면 됨)
        float randomVal = Random.value; // 0.0 ~ 1.0 랜덤

        if (randomVal < diamondChance && y < 5) // 다이아는 아주 깊은 곳(y < 5)에서만
        {
            PlaceBlock(x, y, z, diamondPrefab, ItemType.Diamond, true);
        }
        else if (randomVal < goldChance && y < 10) // 금은 중간 깊이(y < 10)부터
        {
            PlaceBlock(x, y, z, goldPrefab, ItemType.Gold, true);
        }
        else if (randomVal < ironChance) // 철
        {
            PlaceBlock(x, y, z, ironPrefab, ItemType.Iron, true);
        }
        else if (randomVal < coalChance) // 석탄은 돌이 있는 곳이면 어디든
        {
            PlaceBlock(x, y, z, coalPrefab, ItemType.HeavyStone, true);
        }
        else
        {
            // 꽝: 일반 돌
            PlaceBlock(x, y, z, stonePrefab, ItemType.Stone, true);
        }
    }

    // 블록 생성 통합 함수
    private void PlaceBlock(int x, int y, int z, GameObject prefab, ItemType type, bool isMineable)
    {
        if (prefab == null) return;

        var go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{type}_{x}_{y}_{z}";

        // Block 컴포넌트 설정 (Block 스크립트가 있다고 가정)
        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = type;
        b.maxHP = 3; // 광물 종류에 따라 체력을 다르게 줄 수도 있음 (switch문 활용)
        b.dropCount = 1;
        b.mineable = isMineable;
    }

    // 플레이어가 블록을 설치할 때 호출하는 함수 (기존 유지)
    public void PlaceTile(Vector3Int pos, ItemType type)
    {
        GameObject targetPrefab = null;
        switch (type)
        {
            case ItemType.Dirt: targetPrefab = dirtPrefab; break;
            case ItemType.Grass: targetPrefab = grassPrefab; break;
            case ItemType.Water: targetPrefab = waterPrefab; break;
            case ItemType.Stone: targetPrefab = stonePrefab; break;
            case ItemType.Iron: targetPrefab = ironPrefab; break;
                // 필요한 경우 설치 가능한 다른 블록 추가
        }

        if (targetPrefab != null)
        {
            PlaceBlock(pos.x, pos.y, pos.z, targetPrefab, type, true);
        }
    }

    // 스테이지에 따라 바닥 블록을 생성하는 함수
    void CreateStageFloor(int x, int y, int z)
    {
        switch (currentStage)
        {
            case 2: // 2스테이지: 바닥이 석탄
                PlaceBlock(x, y, z, coalPrefab, ItemType.Coal, true);
                // 주의: ItemType.Coal이 맞는지 확인하세요. (코드에선 HeavyStone을 쓰기도 했었음)
                break;

            case 3: // 3스테이지: 바닥이 다이아몬드
                PlaceBlock(x, y, z, diamondPrefab, ItemType.Diamond, true);
                break;

            case 1: // 1스테이지: 일반 돌 (혹은 깰 수 없는 기반암)
            default:
                // 1스테이지는 바닥을 캐서 내려가야 하므로 mineable = true 로 설정하거나,
                // 더 이상 못 내려가게 막으려면 mineable = false 로 설정
                PlaceBlock(x, y, z, stonePrefab, ItemType.Stone, true);
                break;
        }
    }
}