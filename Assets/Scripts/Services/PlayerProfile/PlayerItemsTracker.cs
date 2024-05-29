using System;
using System.Collections.Generic;

[Serializable]
public class PlayerItemsTracker
{
    public List<Guid> ListOfNonVisualizedItems = new();
    public List<Equipment.EquipSlot> ListOfNotVisualizedSlots = new();

    [NonSerialized] public Action OnListChange;
    public void VisualizeItem(Guid itemId)
    {
        if (ListOfNonVisualizedItems.Contains(itemId))
            ListOfNonVisualizedItems.Remove(itemId);

        OnListChange?.Invoke();
    }

    public void VisualizeSlot(Equipment.EquipSlot slot)
    {
        if (ListOfNotVisualizedSlots.Contains(slot))
        {
            ListOfNotVisualizedSlots.Remove(slot);
        }
        var item = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.GetItemOnSlot(slot);

        if (item != null)
        {
            VisualizeItem(item.Uuid);
            return;
        } 
        OnListChange?.Invoke();
    }

    public void Init()
    {
        Services.Container.Resolve<Wallet>().OnNewItemCreate += AddItems;
    }

    public void VisualizeAllItems()
    {
        ListOfNonVisualizedItems.Clear();
    }

    private void AddItems(List<Guid> items)
    {
        var list = new List<Guid>();
        foreach(var itemId in items)
        {
            var item = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(itemId);

            if (item is Equipment equip && Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.GetItemOnSlot(equip.Slot) == item)
            {
                continue;
            }
            if(AddItem(itemId))
            {
                list.Add(itemId);
            }
        }
        
        var playerEquipmentManager = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager;
        foreach(var itemId in list)
        {
            var item = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(itemId) as Equipment;
            if(playerEquipmentManager.EquipItem(item.Uuid, item.Slot))
            {
                ListOfNotVisualizedSlots.Add(item.Slot);
            }
        }
        OnListChange?.Invoke();
    }

    public bool AddItem(Guid itemId)
    {
        if(ListOfNonVisualizedItems.Contains(itemId)) return false;
        
        var item = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(itemId);

        if (item is not IInventoryViewable) return false;

        ListOfNonVisualizedItems.Add(item.Uuid);

        if (item is Equipment equip && !PlayerEquipmentManager.FoodSlots.Contains(equip.Slot))
        {
            var playerEquipmentManager = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager;
            if (playerEquipmentManager.GetItemOnSlot(equip.Slot) == null)
            {
                return true;
            }
        }
        return false;
    }
}