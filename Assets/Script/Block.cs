using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("블록 스탯")]
    public BlockType type = BlockType.Dirt;
    public int maxHP = 3;
    [HideInInspector] public int hp;
    public int dropCount = 1;
    public bool mineable = true;

    private void Awake()
    {
        hp = maxHP;
        if (GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();
        if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag == "Untagged")
            gameObject.tag = "Block";
    }
    public void Hit(int damage, Inventory inven)
    {
        if (!mineable) return;
        hp -= damage;

        if (hp <= 0)
        {
            if (inven != null && dropCount > 0)
            {
                // ★ 핵심 수정: (int)로 숫자로 먼저 바꾼 뒤, (ItemType)으로 다시 바꿉니다.
                // BlockType.Dirt(0) -> 숫자 0 -> ItemType.Dirt(0)
                inven.Add((ItemType)type, dropCount);
            }
            Destroy(gameObject);
        }
    }
}
