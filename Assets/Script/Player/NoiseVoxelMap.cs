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
    public int waterHeight = 10;
    public int maxHeight = 16;
    [SerializeField] float noiseScale = 20f;

    private void Start()
    {
        float offsetX = Random.Range(-9999f, 9999f);
        float offsetZ = Random.Range(-9999f, 9999f);
        Debug.Log(offsetX + " " + offsetZ);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float nx = (x + offsetX) / noiseScale;
                float nz = (z + offsetZ) / noiseScale;

                float noise = Mathf.PerlinNoise(nx, nz);

                int h = Mathf.FloorToInt(noise * maxHeight);

                if (h <= 0)
                    h = 1;

                for (int y = 0; y <= h; y++)
                {
                    if (y == h)
                    {
                        GrassPlace(x, y, z);
                    }
                    else
                    {
                        DirtPlace(x, y, z);
                    }
                }
                for (int y = h; y < water; y++)
                    WaterPlace(x, y, z);

            }
        }
        //Debug.Log((1 + offsetX) / noiseScale);
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
