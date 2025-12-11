using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class NoiseVoxleMap : MonoBehaviour
{
    public GameObject blockPrefab;
    public GameObject waterPrefab;
    public GameObject grassPrefab;

    public int width = 20;
    public int depth = 20;
    public int water = 20;
    public int totalHeight = 40;
    public int maxHeight = 16;
    [Header("Cave Settings")]
    public int floorMaxHeight = 10;  // 바닥의 최대 높이
    public int minCaveHeight = 5;    // 동굴의 최소 높이 (바닥과 천장 사이 간격)
    public int waterLevel = 5;       // 수면 높이

    [SerializeField] float noiseScale = 20f;

    private void Start()
    {
        GenerateCave();
    }

    private void GenerateCave()
    {
        // 바닥용 노이즈 오프셋
        float floorOffsetX = Random.Range(-9999f, 9999f);
        float floorOffsetZ = Random.Range(-9999f, 9999f);

        // 천장용 노이즈 오프셋 (바닥과 모양이 달라야 하므로 별도 생성)
        float ceilingOffsetX = Random.Range(-9999f, 9999f);
        float ceilingOffsetZ = Random.Range(-9999f, 9999f);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 1. 바닥 높이 계산 (기존 로직)
                float nx = (x + floorOffsetX) / noiseScale;
                float nz = (z + floorOffsetZ) / noiseScale;
                float floorNoise = Mathf.PerlinNoise(nx, nz);
                int floorY = Mathf.FloorToInt(floorNoise * floorMaxHeight);
                if (floorY <= 0) floorY = 1;

                // 2. 천장 시작 높이 계산
                // 바닥 높이 + 최소 동굴 높이 + (노이즈 * 추가 높이)
                float cnx = (x + ceilingOffsetX) / noiseScale;
                float cnz = (z + ceilingOffsetZ) / noiseScale;
                float ceilingNoise = Mathf.PerlinNoise(cnx, cnz);

                // 천장은 바닥보다 무조건 minCaveHeight만큼 위에 있어야 함
                int ceilingY = floorY + minCaveHeight + Mathf.FloorToInt(ceilingNoise * 5);

                // 3. Y축 루프: 바닥부터 전체 높이까지
                for (int y = 0; y < totalHeight; y++)
                {
                    // A. 바닥 생성 구역
                    if (y <= floorY)
                    {
                        if (y == floorY && y >= waterLevel) // 물 위라면 잔디
                        {
                            GrassPlace(x, y, z);
                        }
                        else
                        {
                            DirtPlace(x, y, z);
                        }
                    }
                    // B. 물 생성 구역 (바닥보다 위, 수면보다 아래, 천장보다 아래)
                    else if (y > floorY && y < waterLevel && y < ceilingY)
                    {
                        WaterPlace(x, y, z);
                    }
                    // C. 천장 생성 구역 (천장 높이 이상)
                    else if (y >= ceilingY)
                    {
                        DirtPlace(x, y, z);
                    }
                    // D. 그 외(floorY < y < ceilingY)는 공기(빈 공간)
                }
            }
        }
    }
    private void DirtPlace(int x, int y, int z)
    {
        var go = Instantiate(blockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"D_{x}_{y}_{z}";

        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = BlockType.Dirt;
        b.maxHP = 3;
        b.dropCount = 1;
        b.mineable = true;

    }
    private void WaterPlace(int x, int y, int z)
    {
        var go = Instantiate(waterPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"W_{x}_{y}_{z}";

        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = BlockType.Water;
        b.maxHP = 3;
        b.dropCount = 1;
        b.mineable = false;
    }
    private void GrassPlace(int x, int y, int z)
    {
        var go = Instantiate(grassPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"G_{x}_{y}_{z}";

        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = BlockType.Grass;
        b.maxHP = 3;
        b.dropCount = 1;
        b.mineable = true;
    }
    public void PlaceTile(Vector3Int pos, BlockType type)
    {
        switch (type)
        {
            case BlockType.Dirt:
                DirtPlace(pos.x, pos.y, pos.z);
                break;
            case BlockType.Grass:
                GrassPlace(pos.x, pos.y, pos.z);
                break;
            case BlockType.Water:
                WaterPlace(pos.x, pos.y, pos.z);
                break;

        }
    }
}
