using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemDatabaseService
{
    private ItemDatabase itemDatabase;
    public ItemDatabase ItemDatabase => itemDatabase;

    public void Load(Action<bool> callback)
    {
        var loadOp = Resources.LoadAsync<ItemDatabase>("ItemDatabase");
        loadOp.completed += operation =>
        {
            if (operation.isDone)
            {
                itemDatabase = loadOp.asset as ItemDatabase;
            }
            itemDatabase.Start();
            callback?.Invoke(true);
        };
    }
}

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Item/Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> Items;
    [SerializeField] private List<RarityColorPair> rarityColorPairs;
    [SerializeField] private List<RarityColorPair> itemNameRarityColorPairs;
    private Dictionary<Guid, Item> itemDictionary;

    public List<RarityColorPair> ItemNameRarityColorPairs => itemNameRarityColorPairs;
    public List<RarityColorPair> RarityColorPairs => rarityColorPairs;

    private Dictionary<Rarity, string> rarityColorHexa = new();
    public Dictionary<Rarity, string> RarityColorHexa => rarityColorHexa;
    
    private Dictionary<Rarity, string> nameColorHexa = new();
    public Dictionary<Rarity, string> NameColorHexa => nameColorHexa;

    public void Start()
    {
        itemDictionary = new Dictionary<Guid, Item>();
        int i = 0;
        foreach (var item in Items)
        {
            item.Position = i;
            itemDictionary.Add(Guid.Parse(item.id), item);
            i++;
        }
        foreach (var rarityPair in rarityColorPairs)
        {
            string stringRarity = "#" + ColorUtility.ToHtmlStringRGBA(rarityPair.color);
            rarityColorHexa.Add(rarityPair.rarity, stringRarity);
        }
        
        foreach (var rarityPair in ItemNameRarityColorPairs)
        {
            string stringRarity = "#" + ColorUtility.ToHtmlStringRGBA(rarityPair.color);
            nameColorHexa.Add(rarityPair.rarity, stringRarity);
        }
    }

    public Dictionary<Item, long> ToItemDict(Dictionary<Guid, long> itemIdQuantity)
    {
        Dictionary<Item, long> ans = new Dictionary<Item, long>(itemIdQuantity.Count);
        foreach (var kvPair in itemIdQuantity)
        {
            ans.Add(GetItem(kvPair.Key), kvPair.Value);
        }

        return ans;
    }

    public Item GetItem(Guid guid)
    {
        if (!itemDictionary.ContainsKey(guid))
            return null;
        return itemDictionary[guid];
    }
}

[Serializable]
public class RarityColorPair
{
    public Rarity rarity;
    public Color color;
}