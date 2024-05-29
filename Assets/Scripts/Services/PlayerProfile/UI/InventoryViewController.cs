using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryViewController
{
    [SerializeField] private InventoryView inventoryView;

    public static Func<Item, bool> ItemTypeFilter => (item) =>
    {
        return item is IInventoryViewable;
    };

    public void Init()
    {
        inventoryView.Init();
        Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.OnItemAmountChanged += UpdateItemList;
    }

    public void Finish(){
        Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.OnItemAmountChanged -= UpdateItemList;
    }


    public void UpdateItemList(Dictionary<Guid, long> itemsChanged)
    {
        inventoryView.Redraw();
    }
}