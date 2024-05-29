using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropCanvasView : SingletonBehaviour<ItemDropCanvasView>
{
    [SerializeField] private ItemDropView itemDropViewPrefab;
    [SerializeField] private Transform itemViewContainer;
    [SerializeField] private float messageCooldown;

    [Header("Calibration")] 
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform target;

    private ObjectPool<ItemDropView> itemViewPool;

    private Queue<Guid> toShowQueuedItems;
    private Dictionary<Guid, long> accumulatedAmounts;

    private float nextMessageLock;

    private void Start()
    {
        itemViewPool = new ObjectPool<ItemDropView>(itemViewContainer, itemDropViewPrefab);
        toShowQueuedItems = new Queue<Guid>();
        accumulatedAmounts = new Dictionary<Guid, long>();
    }

    private void Update()
    {
        if (Time.time < nextMessageLock)
            return;
        if (toShowQueuedItems.Count == 0)
            return;
        var itemToShowId = toShowQueuedItems.Dequeue();
        var itemToShow = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(itemToShowId);
        if(itemToShow is Experience){
            AddXpDropsView(itemToShow);
            nextMessageLock = Time.time + messageCooldown;
            accumulatedAmounts.Remove(itemToShow.Uuid);
        }else{
            
        }
        
    }

    public void ShowItems(Dictionary<Guid, long> itemsDropped)
    {
        foreach (var itemDropped in itemsDropped)
        {
            if (accumulatedAmounts.ContainsKey(itemDropped.Key))
            {
                accumulatedAmounts[itemDropped.Key] += itemDropped.Value;
            }
            else
            {
                toShowQueuedItems.Enqueue(itemDropped.Key);
                accumulatedAmounts.Add(itemDropped.Key, itemDropped.Value);
            }
        }
    }

    public void AddCurrentDropsView(Dictionary<Guid, long> itemsDropped){

    }

    public void AddXpDropsView(Item item){
        var itemView = itemViewPool.Pop();
        var amount = accumulatedAmounts[item.Uuid];
        itemView.viewData.text = $"+{SkillExperienceTable.Format(amount)}";
        itemView.viewData.item = item;
        itemView.ApplyViewData();
        itemView.ResetData();
        itemView.
        Show(spawnPoint.transform.position, target.transform.position, () =>
        {
            itemView.gameObject.SetActive(false);
            itemViewPool.Push(itemView);
        });
    }
}