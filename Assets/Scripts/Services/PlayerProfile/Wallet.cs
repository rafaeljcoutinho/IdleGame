using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Wallet
{
    public static class RewardSource
    {
        public static Guid AfkGains => Guid.Parse("e6884f0f-e7d6-437e-9363-bcc07fbebde7");
        public static Guid KillstreakChest => Guid.Parse("31549863-1212-46df-a737-5ec48e067d12");
    }

    public const string coinsId = "726f3a06-1b33-4b20-b263-a185a08a3f0f";
    
    public Dictionary<Guid, long> Items;
    public Dictionary<Guid, Dictionary<Guid, long>> PendingRewards;

    public event Action<Dictionary<Guid, long>> OnItemAmountChanged;
    public event Action<List<Guid>> OnNewItemCreate;
    [NonSerialized] private Dictionary<Guid, long> listOfChangedItems = new ();
    [NonSerialized] private List<Guid> listOfCreatedItems = new ();

    public static Wallet Default => new()
    {
        Items = new Dictionary<Guid, long>(),
    };

    public long ItemAmount(Guid itemId, long defau = 0)
    {
        return Items.ContainsKey(itemId) ? Items[itemId] : defau;
    }
    
    public long ItemAmount(string itemId, long defau = 0)
    {
        return ItemAmount(Guid.Parse(itemId), defau);
    }
    
    public bool CanBuy(Dictionary<Guid, long> costs)
    {
        foreach (var cost in costs)
        {
            var itemId = cost.Key;
            var amount = cost.Value;
            if (!CanBuy(itemId, amount))
            {
                return false;
            }
        }
        return true;
    }

    public bool Transaction(Dictionary<Guid, long> costs, Dictionary<Guid, long> rewards)
    {
        if (!CanBuy(costs))
            return false;

        listOfChangedItems.Clear();
        listOfCreatedItems.Clear();
        foreach (var cost in costs)
        {
            Items[cost.Key] -= cost.Value;
            listOfChangedItems.Add(cost.Key, -cost.Value);
        }
        
        foreach (var itemToGive in rewards)
        {
            var itemGuid = itemToGive.Key;
            var amount = itemToGive.Value;
            if (!Items.ContainsKey(itemGuid))
            {
                Items.Add(itemGuid, 0);
                listOfCreatedItems.Add(itemGuid);
            }

            Items[itemGuid] += amount;
            listOfChangedItems.Add(itemGuid, amount);
        }

        if (listOfChangedItems.Count > 0)
        {
            OnItemAmountChanged?.Invoke(listOfChangedItems);
        }
        if (listOfCreatedItems.Count > 0)
        {
            OnNewItemCreate?.Invoke(listOfCreatedItems);
        }
        return true;   
    }
    
    public bool CanBuy(Guid itemId, long amount)
    {
        if (!Items.ContainsKey(itemId))
            return false;
        if (Items[itemId] < amount)
        {
            return false;
        }

        return true;
    }

    public bool SpendItem(Guid id, long cost)
    {
        if (!CanBuy(id, cost))
            return false;
        listOfChangedItems.Clear();
        Items[id] -= cost;
        listOfChangedItems.Add(id, -cost);
        OnItemAmountChanged?.Invoke(listOfChangedItems);
        return true;
    }
    
    public bool SpendItems(Dictionary<Guid, long> costs)
    {
        if (!CanBuy(costs))
            return false;
        listOfChangedItems.Clear();
        foreach (var cost in costs)
        {
            Items[cost.Key] -= cost.Value;
            listOfChangedItems.Add(cost.Key, -cost.Value);
        }
        OnItemAmountChanged?.Invoke(listOfChangedItems);
        return true;
    }

    public void SetItem(Guid id, long amount)
    {
        listOfChangedItems.Clear();
        listOfCreatedItems.Clear();
        if (!Items.ContainsKey(id))
        {
            listOfCreatedItems.Add(id);
            Items.Add(id, 0);
        }
        Items[id] = amount;
        listOfChangedItems.Add(id, amount);
        OnItemAmountChanged?.Invoke(listOfChangedItems);

        if (listOfCreatedItems.Count > 0)
        {
            OnNewItemCreate?.Invoke(listOfCreatedItems);
        }   
    } 
    

    public void GiveItem(Guid id, long amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"Tried to give item {id} with {amount}");
            return;
        }
        listOfChangedItems.Clear();
        listOfCreatedItems.Clear();
        if (!Items.ContainsKey(id))
        {
            listOfCreatedItems.Add(id);
            Items.Add(id, 0);
        }

        Items[id] += amount;
        listOfChangedItems.Add(id, amount);
        OnItemAmountChanged?.Invoke(listOfChangedItems);
        if (listOfCreatedItems.Count > 0)
        {
            OnNewItemCreate?.Invoke(listOfCreatedItems);
        }
    } 
    
    public void AddPendingItems(Guid source, Dictionary<Guid, long> itemsToGive)
    {
        PendingRewards ??= new Dictionary<Guid, Dictionary<Guid, long>>();
        if (!PendingRewards.ContainsKey(source))
        {
            PendingRewards.Add(source, itemsToGive);
        }
        else
        {
            PendingRewards[source] = Utils.Combine(PendingRewards[source], itemsToGive);
        }
    }

    public Dictionary<Guid, long> ApplyPendingItems(Guid source)
    {
        if (PendingRewards == null || !PendingRewards.ContainsKey(source))
        {
            Debug.LogError("Cannot apply pending rewards");
            return null;
        }

        var rewardsToApply = PendingRewards[source];
        PendingRewards.Remove(source);
        GiveItems(rewardsToApply);
        return rewardsToApply;
    }

    public void GiveItems(Dictionary<Guid, long> itemsToGive)
    {
        listOfChangedItems.Clear();
        listOfCreatedItems.Clear();
        foreach (var itemToGive in itemsToGive)
        {
            var itemGuid = itemToGive.Key;
            var amount = itemToGive.Value;
            if (!Items.ContainsKey(itemGuid))
            {
                Items.Add(itemGuid, 0);
                listOfCreatedItems.Add(itemGuid);
            }

            Items[itemGuid] += amount;
            listOfChangedItems.Add(itemGuid, amount);
        }

        if (listOfChangedItems.Count > 0)
        {
            OnItemAmountChanged?.Invoke(listOfChangedItems);
        }

        if (listOfCreatedItems.Count > 0)
        {
            OnNewItemCreate?.Invoke(listOfCreatedItems);
        }
    } 
}