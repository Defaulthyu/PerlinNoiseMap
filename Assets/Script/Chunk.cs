using System.Collections;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private WorldGenerator world;
    private Vector2Int coord;

    public void Init(WorldGenerator worldGen, Vector2Int coordinates)
    {
        this.world = worldGen;
        this.coord = coordinates;
        StartCoroutine(GenerateBlocks());
    }

    IEnumerator GenerateBlocks()
    {
        int startX = coord.x * world.chunkSize;
        int startZ = coord.y * world.chunkSize;

        for (int x = 0; x < world.chunkSize; x++)
        {
            for (int z = 0; z < world.chunkSize; z++)
            {
                int worldX = startX + x;
                int worldZ = startZ + z;

                // 1. 월드 매니저에게 이 좌표의 높이를 물어봄
                int surfaceY = world.GetSurfaceHeight(worldX, worldZ);
                int caveCeilingY = world.GetCaveCeilingHeight(worldX, worldZ, surfaceY);

                for (int y = 0; y < world.totalHeight; y++)
                {
                    Vector3 pos = new Vector3(worldX, y, worldZ);
                    GameObject prefabToSpawn = null;
                    BlockType type = BlockType.Dirt;

                    // 지형 생성 로직
                    if (y <= surfaceY)
                    {
                        if (y == surfaceY && y >= world.waterLevel)
                        {
                            prefabToSpawn = world.grassPrefab;
                            type = BlockType.Grass;
                        }
                        else
                        {
                            prefabToSpawn = world.blockPrefab;
                            type = BlockType.Dirt;
                            if (y < surfaceY - 2)
                            {
                                foreach (var mineral in world.minerals)
                                {
                                    // 1. 높이 조건 체크
                                    if (y >= mineral.minHeight && y <= mineral.maxHeight)
                                    {
                                        // 2. 확률 체크 (Random.value는 0.0 ~ 1.0 사이 랜덤)
                                        if (Random.value < mineral.rarity)
                                        {
                                            prefabToSpawn = mineral.prefab;
                                            type = mineral.type;
                                            break; // 광물 당첨되면 루프 종료 (중복 생성 방지)
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (y > surfaceY && y < world.waterLevel && y < caveCeilingY)
                    {
                        prefabToSpawn = world.waterPrefab;
                        type = BlockType.Water;
                    }
                    else if (y >= caveCeilingY)
                    {
                        prefabToSpawn = world.blockPrefab;
                        type = BlockType.Dirt;
                    }

                    // 블록 생성
                    if (prefabToSpawn != null)
                    {
                        GameObject go = Instantiate(prefabToSpawn, pos, Quaternion.identity, transform);

                        // 블록 컴포넌트 설정 (프리팹에 있다면 생략 가능)
                        Block b = go.GetComponent<Block>();
                        if (b != null) b.type =(ItemType)type;
                    }
                }
            }
            yield return null;
        }
    }
}