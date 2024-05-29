using System;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableItemResolver
{
    public enum Source
    {
        Inventory,
        Equipment,
    }

    private Player player;
    private Source source;
    private Dictionary<Guid, float> NextTimeForFoodItems;

    public ConsumableItemResolver()
    {
        NextTimeForFoodItems = new();
    }
    
    public ConsumableItemResolver SetContext(Player player, Source source)
    {
        this.player = player;
        this.source = source;
        return this;
    }

    public bool ConsumeItem(EquipableFood food)
    {
        var nextTime = 0f;
        if (NextTimeForFoodItems.ContainsKey(food.Uuid))
        {
            nextTime = NextTimeForFoodItems[food.Uuid];
        }

        var isOffCooldown = Time.time >= nextTime;
        if (!isOffCooldown)
        {
            if(source == Source.Inventory)
            {
                OverlayCanvas.Instance.ToastNotificationContainer.Show(new TextLayout.LayoutData
                {
                    LocalizedText = "Item on cooldown. Cannot use it yet."
                });
            }
            return false;   
        }

        switch (source)
        {
            case Source.Equipment:
                var equipManager = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager;
                var slot = equipManager.FindFoodEquipSlot(food);
                if (!slot.HasValue)
                    return false;
                var ok = equipManager.ConsumeItem(food.Uuid, slot.Value, 1);
                if (!ok)
                    return false;
                break;
            case Source.Inventory:
                var wallet = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet;
                ok = wallet.SpendItem(food.Uuid, 1);
                if (!ok)
                    return false;
                break;
        }
        player.HpController.Heal(food.HealingAttributes.HpRecovered);
        if (NextTimeForFoodItems.ContainsKey(food.Uuid))
        {
            NextTimeForFoodItems[food.Uuid] = Time.time + food.HealingAttributes.CooldownSeconds;            
        }
        else
        {
            NextTimeForFoodItems.Add(food.Uuid, Time.time + food.HealingAttributes.CooldownSeconds);
        }
        return true;
    }

    public bool ConsumeItem(FastForwardConsumable ffConsumable)
    {
        if (player.CurrentNode == null)
        {
            OverlayCanvas.Instance.ToastNotificationContainer.Show(new TextLayout.LayoutData
            {
                LocalizedText = "Use it while training a skill"
            });
            return false;
        }
        var nodeData = player.CurrentNode.NodeData;
        var actionNodeData = nodeData as ActionNodeData;

        var behaviour = player.CurrentNode.PlayerBehaviour;
        var skill = player.GetBehaviour(behaviour).SkillUsed;
        var afkProgress = Services.Container.Resolve<SkillService>().GetAfkProgress(skill, actionNodeData, ffConsumable.Seconds);
        var kills = afkProgress.Item1;
        var drops = afkProgress.Item2;
        Services.Container.Resolve<OverkillService>().NotifyEnemyDied(actionNodeData, kills);
        var inventory = Services.Container.Resolve<InventoryService>();
        inventory.PlayerProfile.Wallet.SpendItem(ffConsumable.Uuid, 1);
        ApplyDrops(drops);
        return true;
    }

    void ApplyDrops(Dictionary<Guid, long> itemDrops)
    {
        var inventory = Services.Container.Resolve<InventoryService>();
        inventory.PlayerProfile.Wallet.GiveItems(itemDrops);
        OverlayCanvas.Instance.ShowDrops(itemDrops);
        inventory.Save();
    }
}