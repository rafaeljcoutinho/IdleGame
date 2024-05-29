using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerEquipmentManager
{
    [Serializable]
    public class EquipedItemInfo
    {
        public Guid Id;
        public long Amount;
    }
    
    public Dictionary<Equipment.EquipSlot, EquipedItemInfo> EquippedItems;
    public event Action<EquippedItemsChangedArgs> OnEquippedItemChanged;

    public class EquippedItemsChangedArgs
    {
        public Equipment.EquipSlot Slot;
        public EquipedItemInfo UnequippedItem;
        public EquipedItemInfo EquippedItem;
    }

    [NonSerialized]public static readonly List<Equipment.EquipSlot> FoodSlots = new ()
    {
        Equipment.EquipSlot.Food_1,
        Equipment.EquipSlot.Food_2,
        Equipment.EquipSlot.Food_3,
        Equipment.EquipSlot.Food_4,
    };
    [NonSerialized]public static readonly List<Equipment.EquipSlot> EquipmentSlots = new ()
    {
        Equipment.EquipSlot.MainHand,
    };
    [NonSerialized]public static readonly List<Equipment.EquipSlot> EquipmentSlotsOnly = new ()
    {
        Equipment.EquipSlot.Helm,
        Equipment.EquipSlot.Body,
        Equipment.EquipSlot.Legs,
        Equipment.EquipSlot.Boots,
        Equipment.EquipSlot.Gloves,
        Equipment.EquipSlot.Cape,
    };
    [NonSerialized]public static readonly List<Equipment.EquipSlot> ToolbeltSlots = new ()
    {
        Equipment.EquipSlot.ToolBeltAxe,
        Equipment.EquipSlot.ToolBeltFishingRod,
        Equipment.EquipSlot.ToolBeltPickaxe,
    };

    [NonSerialized]private EquippedItemsChangedArgs cachedItemEquipArgs;
    private EquippedItemsChangedArgs CachedItemEquipArgs => cachedItemEquipArgs ??= new EquippedItemsChangedArgs();

    public static PlayerEquipmentManager Default => new PlayerEquipmentManager
    {
        EquippedItems = new Dictionary<Equipment.EquipSlot, EquipedItemInfo>(),
    };

    public Item GetItemOnSlot(Equipment.EquipSlot slot)
    {
        if (!EquippedItems.ContainsKey(slot))
            return null;
        
        var itemDatabaseService = Services.Container.Resolve<ItemDatabaseService>();
        return itemDatabaseService.ItemDatabase.GetItem(EquippedItems[slot].Id);
    }

    public long GetQuantityOnSlot(Equipment.EquipSlot slot)
    {
        if (!EquippedItems.ContainsKey(slot))
            return 0;
        
        return EquippedItems[slot].Amount;
    }

    public bool IsFoodSlot(Equipment.EquipSlot slot)
    {
        return FoodSlots.Contains(slot);
    }
    public bool IsEquipmentSlot(Equipment.EquipSlot slot)
    {
        return EquipmentSlots.Contains(slot);
    }
    public bool IsToolbeltSlot(Equipment.EquipSlot slot)
    {
        return ToolbeltSlots.Contains(slot);
    }

    public Equipment.EquipSlot? FindFoodEquipSlot(EquipableFood food)
    {
        foreach (var slot in FoodSlots)
        {
            if (EquippedItems.ContainsKey(slot) &&
                EquippedItems[slot].Id == food.Uuid)
                return slot;            
        }
        return null;
    }
    
    public List<Item> GetAllEquippedItems()
    {
        var ans = new List<Item>();
        foreach (var kvPair in EquippedItems)
        {
            ans.Add(GetItemOnSlot(kvPair.Key));
        }

        return ans;
    }

    public bool ConsumeItem(Guid id, Equipment.EquipSlot slot, long quantity)
    {
        if (!FoodSlots.Contains(slot))
        {
            return false;
        }

        if (!EquippedItems.ContainsKey(slot))
        {
            return false;
        }

        if (EquippedItems[slot].Amount < quantity)
        {
            return false;
        }

        EquippedItems[slot].Amount -= quantity;

        CachedItemEquipArgs.UnequippedItem = null;
        
        if (EquippedItems[slot].Amount == 0)
        {
            CachedItemEquipArgs.UnequippedItem = EquippedItems[slot];
            EquippedItems.Remove(slot);
        }

        Services.Container.Resolve<InventoryService>().Save();
        CachedItemEquipArgs.EquippedItem = null;
        CachedItemEquipArgs.Slot = slot;
        OnEquippedItemChanged?.Invoke(CachedItemEquipArgs);
        return true;
    }
    
    public bool CanEquipItem(Guid id, Equipment.EquipSlot slot, long quantity)
    {
        var item = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(id);
        if (item == null)
        {
            return false;
        }

        var equipment = item as Equipment;
        if (equipment == null)
        {
            return false;
        }

        var meetsRequirements = Services.Container.Resolve<InventoryService>().PlayerProfile.MeetsRequirement(equipment.Requirements);
        if (!meetsRequirements.HasRequirements)
        {
            return false;
        }

        return Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.ItemAmount(id) >= quantity;
    }

    public bool UnequipItem(Equipment.EquipSlot slot)
    {
        if (!EquippedItems.ContainsKey(slot))
            return false;
        
        var wallet = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet;
        var item = EquippedItems[slot];
        if (item == null)
        {
            return false;
        }

        wallet.GiveItem(item.Id, item.Amount);
        EquippedItems.Remove(slot);
        Services.Container.Resolve<InventoryService>().Save();
        
        CachedItemEquipArgs.EquippedItem = null;
        CachedItemEquipArgs.UnequippedItem = item;
        CachedItemEquipArgs.Slot = slot;
        OnEquippedItemChanged?.Invoke(CachedItemEquipArgs);
        return true;
    }

    public bool EquipItem(Guid id, Equipment.EquipSlot slot, long quantity = 1)
    {
        if (!CanEquipItem(id, slot, quantity))
        {
            Debug.LogError("Player cannot equip item");
            return false;
        }

        var walletAmount = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.ItemAmount(id);
        var oldItem = EquippedItems.ContainsKey(slot) ? EquippedItems[slot] : null;
        if (!EquippedItems.ContainsKey(slot))
        {
            EquippedItems.Add(slot, new EquipedItemInfo
            {
                Id = id,
                Amount = 0,
            });
        }

        if (EquippedItems[slot].Id == id)
        {
            EquippedItems[slot].Amount += quantity;
        }
        if (EquippedItems[slot].Id != id)
        {
            EquippedItems[slot] = new EquipedItemInfo
            {
                Id = id,
                Amount = quantity,
            };
        }

        CachedItemEquipArgs.EquippedItem = EquippedItems[slot];
        CachedItemEquipArgs.UnequippedItem = oldItem?.Id == id ? null : oldItem;
        CachedItemEquipArgs.Slot = slot;

        var wallet = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet;
        wallet.SpendItem(id, quantity);

        if (oldItem != null && oldItem.Id != id)
        {
            wallet.GiveItem(oldItem.Id, oldItem.Amount);
        }

        OnEquippedItemChanged?.Invoke(CachedItemEquipArgs);
        Services.Container.Resolve<InventoryService>().Save();
        return true;
    }
}