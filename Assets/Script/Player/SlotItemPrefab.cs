using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotItemPrefab : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI itemText;
    public ItemType itemType;
    public void ItemSetting(Sprite itemSprite, string txt, ItemType type)
    {
        itemImage.sprite = itemSprite;
        itemType = type;
        itemText.text = txt;
    }
}
