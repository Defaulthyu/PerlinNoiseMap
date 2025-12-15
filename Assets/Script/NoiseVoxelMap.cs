using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseVoxleMap : MonoBehaviour
{
    [Header("Prefabs (Must have Block Component)")]
    public GameObject blockPrefab; // Block 스크립트가 붙어있어야 함
    public GameObject waterPrefab;
    public GameObject grassPrefab;

    [Header("Map Settings")]
    public int width = 20;
    public int depth = 20;
    public int totalHeight = 40;

    [Header("Terrain Settings")]
    public int floorMaxHeight = 16;  // 지표면 최대 높이
    public int waterLevel = 8;       // 수면 높이

    [Header("Cave Settings")]
    public int caveMinGap = 4;       // 동굴 최소 높이
    public bool generateCave = true; // 동굴 생성 여부 토글

    [Header("Noise Settings")]
    [SerializeField] float noiseScale = 20f;
    [SerializeField] float seedOffset; // 매번 다른 맵을 위해

    // 블록 데이터를 관리할 딕셔너리 (나중에 채굴/저장 기능을 위해 필요)
    private Dictionary<Vector3Int, Block> blockData = new Dictionary<Vector3Int, Block>();

    private void Start()
    {
        // 랜덤 시드 설정
        seedOffset = Random.Range(-10000f, 10000f);
        GenerateMap();
    }

    private void GenerateMap()
    {
        blockData.Clear(); // 재생성 시 초기화

        // 루프 순서 최적화 (캐시 효율성)
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 1. 지표면(Floor) 높이 계산
                float nx = (x + seedOffset) / noiseScale;
                float nz = (z + seedOffset) / noiseScale;

                // PerlinNoise는 0~1 반환 -> 높이로 변환
                int surfaceY = Mathf.FloorToInt(Mathf.PerlinNoise(nx, nz) * floorMaxHeight);
                if (surfaceY < 1) surfaceY = 1; // 최소 바닥 1칸 보장

                // 2. 동굴 천장(Ceiling) 높이 계산 (동굴이 지표면보다 위에 생기는 것 방지 로직 추가 가능)
                int caveCeilingY = 0;
                if (generateCave)
                {
                    float cnx = (x + seedOffset + 500f) / noiseScale; // 오프셋을 다르게 줌
                    float cnz = (z + seedOffset + 500f) / noiseScale;
                    int noiseHeight = Mathf.FloorToInt(Mathf.PerlinNoise(cnx, cnz) * 10); // 천장 굴곡

                    // 지표면 + 최소 공간 + 굴곡
                    caveCeilingY = surfaceY + caveMinGap + noiseHeight;
                }
                else
                {
                    caveCeilingY = totalHeight + 10; // 동굴 안 만들거면 천장을 맵 밖으로
                }

                // 3. Y축 채우기
                for (int y = 0; y < totalHeight; y++)
                {
                    // A. 지표면 및 지하 (땅)
                    if (y <= surfaceY)
                    {
                        if (y == surfaceY && y >= waterLevel)
                        {
                            CreateBlock(x, y, z, BlockType.Grass, grassPrefab);
                        }
                        else
                        {
                            CreateBlock(x, y, z, BlockType.Dirt, blockPrefab);
                        }
                    }
                    // B. 물 (지표면보다 높고, 수면보다 낮고, 동굴 천장 아래일 때 - 동굴 침수 방지 로직 필요시 수정)
                    else if (y > surfaceY && y < waterLevel && y < caveCeilingY)
                    {
                        CreateBlock(x, y, z, BlockType.Water, waterPrefab);
                    }
                    // C. 동굴 천장 위 (다시 흙)
                    else if (y >= caveCeilingY)
                    {
                        CreateBlock(x, y, z, BlockType.Dirt, blockPrefab);
                    }
                }
            }
        }
    }

    private void CreateBlock(int x, int y, int z, BlockType type, GameObject prefab)
    {
        // 쿼터니언 idnetity는 캐싱된 값을 쓰는게 미세하게 빠름
        var go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{type}_{x}_{y}_{z}";

        // GetComponent 대신, 프리팹에 이미 붙어있는 컴포넌트를 활용하거나
        // 필요하다면 여기서 초기화. (프리팹에 Block 스크립트가 붙어있다고 가정)
        Block b = go.GetComponent<Block>();
        if (b != null)
        {
            b.type = type;
            b.maxHP = 3;
            b.mineable = (type != BlockType.Water);

            // 딕셔너리에 등록 (나중에 블록 찾을 때 유용)
            blockData.Add(new Vector3Int(x, y, z), b);
        }
    }
}