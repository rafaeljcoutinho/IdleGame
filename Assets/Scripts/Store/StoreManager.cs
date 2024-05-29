
using System;
using System.Collections.Generic;
[Serializable]
public class StoreManager
{
    public Dictionary<Guid, Dictionary<Guid, long>> StorePurchasedItems;
    public Dictionary<Guid, long> StoreLastUpdate;

    [NonSerialized] public Action OnStoreChange;
    [NonSerialized] private Dictionary<Guid, long> costs = new();
    [NonSerialized] private Dictionary<Guid, long> rewards = new();


    public void Init()
    {
        StorePurchasedItems ??= new Dictionary<Guid, Dictionary<Guid, long>>();
        StoreLastUpdate ??= new Dictionary<Guid, long>();
    }

    public void Update(Store store)
    {
        LoadStore(Guid.Parse(store.id));
        var storeViewData = new List<StoreItemViewData>();
        var lastStoreUpdate = StoreLastUpdate[Guid.Parse(store.id)];
        bool refresh = false;

        DateTime currentUTC = DateTime.UtcNow;
        //DayOfWeek currentDayOfWeek = currentUTC.DayOfWeek;

        var lastDateTime = StoreLastUpdate[Guid.Parse(store.id)];

        if (store.refreshType == RefreshType.Day)
        {
            if (lastDateTime <= DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds)
            {
                refresh = true;
                DateTime nextMidnight = currentUTC.AddDays(1).AddHours(-currentUTC.Hour).AddMinutes(-currentUTC.Minute).AddSeconds(-currentUTC.Second);
                StoreLastUpdate[Guid.Parse(store.id)] = (long)nextMidnight.Subtract(DateTime.UnixEpoch).TotalSeconds;
            }
        }
        else if (store.refreshType == RefreshType.Hour)
        {
            if (lastDateTime <= DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds)
            {
                refresh = true;
                DateTime nextHour = currentUTC.AddHours(1).AddMinutes(-currentUTC.Minute).AddSeconds(-currentUTC.Second);
                StoreLastUpdate[Guid.Parse(store.id)] = (long)nextHour.Subtract(DateTime.UnixEpoch).TotalSeconds;
            }
        }
        else if (store.refreshType == RefreshType.Minute)
        {
            if (lastDateTime <= DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds)
            {
                refresh = true;
                DateTime nextMinute = currentUTC.AddMinutes(1).AddSeconds(-currentUTC.Second);
                StoreLastUpdate[Guid.Parse(store.id)] = (long)nextMinute.Subtract(DateTime.UnixEpoch).TotalSeconds;
            }
        }
        if (refresh)
        {
            StorePurchasedItems[Guid.Parse(store.id)].Clear();
            OnStoreChange?.Invoke();
        }
    }
    public List<StoreItemViewData> GetStoreViewData(Store store)
    {
        LoadStore(Guid.Parse(store.id));
        var storeViewData = new List<StoreItemViewData>();
        
        foreach (var storeItem in store.storeItems)
        {
            if(StorePurchasedItems[Guid.Parse(store.id)].ContainsKey(Guid.Parse(storeItem.id)))
            {
                storeViewData.Add(new StoreItemViewData{
                    id = storeItem.id,
                    item = storeItem.item,
                    quantityEnabled = storeItem.quantityEnabled - StorePurchasedItems[Guid.Parse(store.id)][Guid.Parse(storeItem.id)] ,
                    price = storeItem.price,
                    isFree = storeItem.isFree,
                    discount = storeItem.discount,
                });
            }
            else
            {
                storeViewData.Add(new StoreItemViewData{
                    id = storeItem.id,
                    item = storeItem.item,
                    quantityEnabled = storeItem.quantityEnabled,
                    price = storeItem.price,
                    isFree = storeItem.isFree,
                    discount = storeItem.discount,
                });
            }
        }

        return storeViewData;
    }

    public void LoadStore(Guid storeId)
    {
        if (!StorePurchasedItems.ContainsKey(storeId))
        {
            StorePurchasedItems.Add(storeId, new());
        }

        if (!StoreLastUpdate.ContainsKey(storeId))
        {
            StoreLastUpdate.Add(storeId, (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds);
        }
    }

    public void BuyItem(Store store, StoreItemViewData storeItemData, long quantity)
    {
        
        var wallet = Services.Container.Resolve<Wallet>();
        costs.Clear();
        rewards.Clear();

        costs.Add(storeItemData.price.item.Uuid, storeItemData.price.quantity * quantity);
        rewards.Add(storeItemData.item.Uuid, quantity);
        OverlayCanvas.Instance.ShowDrops(rewards);
        
        wallet.Transaction(costs, rewards);

        if (!StorePurchasedItems[Guid.Parse(store.id)].ContainsKey(Guid.Parse(storeItemData.id)))
        {
            StorePurchasedItems[Guid.Parse(store.id)].Add(Guid.Parse(storeItemData.id), quantity);
        }
        else
        {
            StorePurchasedItems[Guid.Parse(store.id)][Guid.Parse(storeItemData.id)] += quantity;
        }
        OnStoreChange?.Invoke();
    }
    public void ClaimItem(Store store, StoreItemViewData storeItemData, long quantity)
    {
        
        var wallet = Services.Container.Resolve<Wallet>();

        rewards.Clear();
        rewards.Add(storeItemData.item.Uuid, quantity);
        
        wallet.GiveItem(storeItemData.item.Uuid, quantity);
        OverlayCanvas.Instance.ShowDrops(rewards);

        if (!StorePurchasedItems[Guid.Parse(store.id)].ContainsKey(Guid.Parse(storeItemData.id)))
        {
            StorePurchasedItems[Guid.Parse(store.id)].Add(Guid.Parse(storeItemData.id), quantity);
        }
        else
        {
            StorePurchasedItems[Guid.Parse(store.id)][Guid.Parse(storeItemData.id)] += quantity;
        }
        OnStoreChange?.Invoke();
    }
}
