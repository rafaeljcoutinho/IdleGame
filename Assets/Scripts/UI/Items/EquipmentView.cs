using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static PlayerEquipmentManager;

public class EquipmentView : MonoBehaviour
{
    public class ViewData {}
    
    [Serializable]
    public class ItemSlotPair
    {
        public Equipment.EquipSlot slot;
        public ItemIconPrefab itemIcon;
    }
    [SerializeField] private List<ItemSlotPair> equipmentSlotPairs;
    [SerializeField] private StatsQuickView statsQuickView;



    private void OnEnable() {
        Setup(null);
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged += Setup;
    }

    private void OnDisable() {
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged -= Setup;
    }


    public void ShowStats()
    {
        
    }
    
    public void Setup(EquippedItemsChangedArgs OnEquippedItemChanged){
        var equipmentManager = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager;
        
        foreach(var equipSlot in equipmentSlotPairs)
        {
            var item = equipmentManager.GetItemOnSlot(equipSlot.slot);
            if(item == null) continue;

            equipSlot.itemIcon.SetData(new InventoryItemViewData
                { 
                    itemData = equipmentManager.GetItemOnSlot(equipSlot.slot), 
                    itemQuantity = equipmentManager.GetQuantityOnSlot(equipSlot.slot),
                }, true);
            equipSlot.itemIcon.SetButton(OnItemClick, equipSlot);
        }
    }


    private void OnItemClick(ItemSlotPair pair)
    {
        pair.itemIcon.SetRedDot(false);
        OverlayCanvas.Instance.ToolTip.Show(pair.itemIcon.InventoryItemViewData.itemData, pair.itemIcon.transform.position, ConsumableItemResolver.Source.Inventory, true);
        OverlayCanvas.Instance.ToolTip.SetEquipedItemButton(()=> OnRemoveItem(pair));
    }

    private void OnRemoveItem(ItemSlotPair pair)
    {
        var equipmentManager = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager;

        if (equipmentManager.GetItemOnSlot(pair.slot) != null)
        {
            pair.itemIcon.ClearEquipItemData();
            equipmentManager.UnequipItem(pair.slot);
            return;
        }
    }

}
