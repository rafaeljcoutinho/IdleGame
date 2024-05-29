using System;
using UnityEngine;

[CreateAssetMenu(fileName ="Item", menuName = "Item/BasicItem")]
public class Item : ScriptableObject
{
    [ScriptableObjectId] public string id;

    public Guid Uuid => Guid.Parse(id);
    public Sprite SmallThumbnail;
    public int ItemLv;
    public string LocalizationKey;
    [TextArea]
    public string DescriptionLocalizationKey;
    public Rarity rarity;
    public int Position;

    public virtual bool ShowInInventory => false;


    [ContextMenu("ResetGuid")]
    public void ResetGuid()
    {
        id = Guid.NewGuid().ToString();
    }
    
    public virtual bool Accept(ConsumableItemResolver itemResolver)
    {
        return false;
    }
}

public enum Rarity{
    Commom,
    Uncommom,
    Rare,
    Epic,
    Legendary,
}