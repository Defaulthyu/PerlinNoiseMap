using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Mineral
{
    public string name;         // 이름 (구분용)
    public GameObject prefab;   // 광물 프리팹
    public BlockType type;      // 블록 타입
    [Range(0f, 1f)]
    public float rarity;        // 등장 확률 (0.01 = 1%)
    public int minHeight;       // 생성 최소 높이 (예: 0)
    public int maxHeight;       // 생성 최대 높이 (예: 20)
}

public class WorldGenerator : MonoBehaviour
{
    [Header("플레이어")]
    public Transform player;       // 플레이어 트랜스폼 연결
    public bool autoSpawnPlayer = true;

    [Header("블록 프리펩")]
    public GameObject blockPrefab;
    public GameObject waterPrefab;
    public GameObject grassPrefab;
    
    [Header("Ores")]
    public Mineral[] minerals;

    [Header("월드 설정")]
    public int chunkSize = 16;     // 한 청크의 크기
    public int renderDistance = 3; // 시야 거리
    public int totalHeight = 40;   // 맵 전체 높이 제한

    [Header("노이즈 설정")]
    public int floorMaxHeight = 16;
    public int waterLevel = 8;
    public float noiseScale = 20f;
    public int seed;


    // 현재 생성된 청크들을 관리하는 딕셔너리 (좌표 -> 청크객체)
    private Dictionary<Vector2Int, Chunk> activeChunks = new Dictionary<Vector2Int, Chunk>();
    private Vector2Int playerChunkCoord;
    private bool isFirstInit = true;

    private void Start()
    {
        // 랜덤 시드 설정
        seed = Random.Range(-10000, 10000);

        // 초기 청크 생성 및 플레이어 배치
        UpdateChunks();

        if (autoSpawnPlayer && player != null)
        {
            SpawnPlayerSafely();
        }
    }

    private void Update()
    {
        if (player == null) return;

        // 플레이어가 있는 현재 청크 좌표 계산
        Vector2Int currentChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.z / chunkSize)
        );

        // 플레이어가 다른 청크로 이동했을 때만 업데이트
        if (currentChunkCoord != playerChunkCoord || isFirstInit)
        {
            playerChunkCoord = currentChunkCoord;
            UpdateChunks();
            isFirstInit = false;
        }
    }

    // 플레이어 주변 청크 생성 및 먼 청크 삭제
    void UpdateChunks()
    {
        List<Vector2Int> chunksToKeep = new List<Vector2Int>();

        // 1. 시야 범위 내의 청크 생성
        for (int xOffset = -renderDistance; xOffset <= renderDistance; xOffset++)
        {
            for (int zOffset = -renderDistance; zOffset <= renderDistance; zOffset++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunkCoord.x + xOffset, playerChunkCoord.y + zOffset);
                chunksToKeep.Add(chunkCoord);

                // 이미 있는 청크면 패스, 없으면 새로 생성
                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    CreateChunk(chunkCoord);
                }
            }
        }

        // 2. 시야 밖으로 벗어난 청크 삭제 (메모리 관리)
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var key in activeChunks.Keys)
        {
            if (!chunksToKeep.Contains(key))
            {
                chunksToRemove.Add(key);
            }
        }

        foreach (var key in chunksToRemove)
        {
            Chunk chunk = activeChunks[key];
            Destroy(chunk.gameObject); // 게임 오브젝트 삭제
            activeChunks.Remove(key);  // 리스트에서 제거
        }
    }

    void CreateChunk(Vector2Int coord)
    {
        GameObject chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}");
        chunkObj.transform.parent = this.transform;
        chunkObj.transform.position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        Chunk newChunk = chunkObj.AddComponent<Chunk>();

        // 청크에 필요한 정보 전달 및 생성 시작
        newChunk.Init(this, coord);

        activeChunks.Add(coord, newChunk);
    }

    // 플레이어를 안전하게 땅 위에 배치하는 함수
    void SpawnPlayerSafely()
    {
        // (0,0) 좌표의 지형 높이를 계산
        int spawnX = 0;
        int spawnZ = 0;
        int terrainHeight = GetSurfaceHeight(spawnX, spawnZ);

        // 높이보다 2칸 위에 배치 (끼임 방지)
        Vector3 spawnPos = new Vector3(spawnX, terrainHeight + 1f, spawnZ);
    }

    // 특정 월드 좌표(x, z)의 지표면 높이를 계산하는 함수 (외부에서도 사용 가능)
    public int GetSurfaceHeight(int worldX, int worldZ)
    {
        float nx = (worldX + seed) / noiseScale;
        float nz = (worldZ + seed) / noiseScale;

        int height = Mathf.FloorToInt(Mathf.PerlinNoise(nx, nz) * floorMaxHeight);
        if (height < 1) height = 1;

        return height;
    }

    // 동굴 천장 높이 계산 (필요시)
    public int GetCaveCeilingHeight(int worldX, int worldZ, int surfaceHeight)
    {
        float cnx = (worldX + seed + 500f) / noiseScale;
        float cnz = (worldZ + seed + 500f) / noiseScale;
        int noiseHeight = Mathf.FloorToInt(Mathf.PerlinNoise(cnx, cnz) * 10);

        // 최소 갭 4칸
        return surfaceHeight + 4 + noiseHeight;
    }

    public void PlaceBlock(Vector3Int pos, BlockType type)
    {
        // 1. 어떤 프리팹을 쓸지 결정
        GameObject prefabToUse = null;
        switch (type)
        {
            case BlockType.Dirt: prefabToUse = blockPrefab; break;
            case BlockType.Grass: prefabToUse = grassPrefab; break;
            case BlockType.Water: prefabToUse = waterPrefab; break;
                // 광물은 minerals 배열에서 찾거나 따로 설정해야 합니다.
                // 예시: 
                // case BlockType.Coal: prefabToUse = minerals[0].prefab; break; 
        }

        if (prefabToUse != null)
        {
            // 2. 해당 좌표가 속한 청크 찾기 (부모 정해주기 위해)
            Chunk targetChunk = GetChunkAt(pos);
            Transform parentTransform = targetChunk != null ? targetChunk.transform : transform;

            // 3. 생성
            GameObject go = Instantiate(prefabToUse, pos, Quaternion.identity, parentTransform);

            // 4. 블록 정보 세팅
            Block b = go.GetComponent<Block>();
            if (b != null)
            {
                b.type = type;
            }
        }
    }

    // 좌표로 청크를 찾는 헬퍼 함수
    private Chunk GetChunkAt(Vector3Int pos)
    {
        int chunkX = Mathf.FloorToInt((float)pos.x / chunkSize);
        int chunkZ = Mathf.FloorToInt((float)pos.z / chunkSize); // y대신 z 주의
        Vector2Int coord = new Vector2Int(chunkX, chunkZ);

        if (activeChunks.TryGetValue(coord, out Chunk chunk))
        {
            return chunk;
        }
        return null;
    }
}