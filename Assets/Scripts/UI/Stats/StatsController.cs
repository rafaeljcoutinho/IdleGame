using System;
using System.Collections.Generic;
using UnityEngine;

public class StatsController : MonoBehaviour
{
    private ActionProgressParameters currentPlayerStats;

    private ActionProgressParameters nextPlayerStats;

    public ActionProgressParameters GetCurrentPlayerStats(SkillData.Type skill)
    {
        currentPlayerStats = Services.Container.Resolve<SkillService>().GetCombinedProgressParameters(skill, null);
        return currentPlayerStats;
    }

    public ActionProgressParameters GetNextPlayerStats(SkillData.Type skill, Guid previousItemId, Guid nextItemId)
    {
        var inventoryService = Services.Container.Resolve<InventoryService>();

        var previousItem = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(previousItemId);
        var nextItem = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(nextItemId);

        List<Item> newEquipedItems = new();

        foreach (var item in inventoryService.PlayerProfile.PlayerEquipmentManager.GetAllEquippedItems())
        {
            var itemToCalculate = item;
            if (item == previousItem)
            {
                itemToCalculate = nextItem;
            }

            newEquipedItems.Add(itemToCalculate);
        }
        return nextPlayerStats;
    }
}