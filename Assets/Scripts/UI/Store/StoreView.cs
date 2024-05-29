using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoreView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI storeTitle;
    [SerializeField] private Transform itemsParent;
    [SerializeField] private ItemIconPrefab itemPrefab;
    [SerializeField] private TextMeshProUGUI timeToRefreshStore;

    private List<GameObject> allPrefabs = new();    
    private StoreItemViewData clickedItem;
    private Store store;
    private bool isShowing;

    public void Show(List<StoreItemViewData> storeItemViewDatas, Store store)
    {
        this.store = store;
        isShowing = true;
        
        foreach (var item in storeItemViewDatas)
        {
            var go = Instantiate(itemPrefab, itemsParent);
            allPrefabs.Add(go.gameObject);
            go.SetData(new InventoryItemViewData{
                itemData = item.item,
                itemQuantity = item.quantityEnabled,
            });
            
            if (item.discount != 0)
            {
                go.EnableDiscount(item.discount);
            }
            if (item.isFree)
            {
                go.EnableFree(OnClaimItem);
                go.SetButton(OnFreeItemClick);
            }
            else
            {
                go.SetButton(OnItemClick);                 
            }
                   
            go.SetStoreData(item);
        }
        Update();
        gameObject.SetActive(true);
    }

    private void Update() {
        if (!isShowing && store != null) return;

        Services.Container.Resolve<InventoryService>().PlayerProfile.storeManager.Update(store);
        DateTime currentUTC = DateTime.UtcNow;


        DateTime nextMidnight = currentUTC.AddDays(1).AddHours(-currentUTC.Hour).AddMinutes(-currentUTC.Minute).AddSeconds(-currentUTC.Second);
        double hoursUntilNextMidnight = (nextMidnight - currentUTC).TotalHours;

        DateTime nextHour = currentUTC.AddHours(1).AddMinutes(-currentUTC.Minute).AddSeconds(-currentUTC.Second);
        double minutesUntilNextHour = (nextHour - currentUTC).TotalMinutes;

        DateTime nextMinute = currentUTC.AddMinutes(1).AddSeconds(-currentUTC.Second);
        double secondsUntilNextMinute = Math.Ceiling((nextMinute - currentUTC).TotalSeconds);

        DayOfWeek currentDayOfWeek = currentUTC.DayOfWeek;
        int daysUntilNextWeek = (int)DayOfWeek.Saturday - (int)currentDayOfWeek;
        if (daysUntilNextWeek <= 0)
            daysUntilNextWeek += 7;

        if (store.refreshType == RefreshType.Minute)
        {
            timeToRefreshStore.text = string.Format("{0:00}", (int)secondsUntilNextMinute);
        }
        else if (store.refreshType == RefreshType.Hour)
        {
            timeToRefreshStore.text = string.Format("{0:00}:{1:00}",  (int)minutesUntilNextHour, (int)secondsUntilNextMinute);
        }
        else if (store.refreshType == RefreshType.Day)
        {
            timeToRefreshStore.text = string.Format("{0:00}:{1:00}:{2:00}", (int)hoursUntilNextMidnight, (int)minutesUntilNextHour, (int)secondsUntilNextMinute);
        }

    }

    private void RefreshUI()
    {
        ClearData();
        var storeViewData = Services.Container.Resolve<InventoryService>().PlayerProfile.storeManager.GetStoreViewData(store);
        Show(storeViewData, store);
    }

    private void OnEnable() 
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.storeManager.OnStoreChange += RefreshUI;
    }
    private void OnDisable() {
        isShowing = false;
        Services.Container.Resolve<InventoryService>().PlayerProfile.storeManager.OnStoreChange -= RefreshUI;
    }


    public void OnItemClick(StoreItemViewData storeItemViewData, Vector3 position)
    {
        clickedItem = storeItemViewData;
        var item = storeItemViewData.item;
        var maxQuantity = storeItemViewData.quantityEnabled;
        var price = storeItemViewData.price;
        var walletAmount = Services.Container.Resolve<Wallet>().ItemAmount(price.item.Uuid);

        var canBuyQuantity = Math.Min(maxQuantity, walletAmount / price.quantity);

        OverlayCanvas.Instance.ToolTip.ShowQuantitySelector(price, maxQuantity, canBuyQuantity, ActionType.Buy, OnBuyItem);
        OverlayCanvas.Instance.ToolTip.Show(item, position, ConsumableItemResolver.Source.Inventory, true);
    }

    public void OnFreeItemClick(StoreItemViewData storeItemViewData, Vector3 position)
    {
        clickedItem = storeItemViewData;
        var item = storeItemViewData.item;

        OverlayCanvas.Instance.ToolTip.Show(item, position, ConsumableItemResolver.Source.Inventory, true);
    }

    private void OnBuyItem(long quantity)
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.storeManager.BuyItem(store, clickedItem, quantity);
    }
    private void OnClaimItem(StoreItemViewData item)
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.storeManager.ClaimItem(store, item, item.quantityEnabled);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ClearData();
    }

    private void ClearData()
    {
        foreach(var go in allPrefabs)
        {
            Destroy(go);
        }
        allPrefabs.Clear();
    }
}

public class StoreItemViewData
{    
    public string id;
    public bool isFree;
    public Item item;
    public long quantityEnabled;
    public ItemWithQuantity price;
    public int discount;
}