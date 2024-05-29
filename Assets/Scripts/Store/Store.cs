using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="StoreData", menuName = "Store/StoreData")]

public class Store : ScriptableObject
{
    [ScriptableObjectId] public string id;
    public List<StoreItem> storeItems;
    public RefreshType refreshType;

    private void OnValidate() {
        List<string> ids = new();
        foreach (var item in storeItems)
        {
            if (ids.Contains(item.id))
            {
                item.id = Guid.NewGuid().ToString();
            }
            ids.Add(item.id);
        }
    }
}



[Serializable]
public class StoreItem
{
    [ScriptableObjectId] public string id;
    public Item item;
    public bool isFree;
    public long quantityEnabled;
    public ItemWithQuantity price;
    [Range(0,100)]
    public int discount;
}

public enum RefreshType
{
    Second,
    Minute,
    Hour,
    Day,
    Month
}