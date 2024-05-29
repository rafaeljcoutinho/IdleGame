using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cooking
{
    [Serializable]
    public class CookingManager
    {
        [Serializable]
        public class CookingSummary
        {
            public long LastCookStartTime;
            public long TotalCookedItems;
            public Dictionary<Guid, long> Ins;
            public Dictionary<Guid, long> Outs;
        }
        
        public List<CookingRecord> CookingRecords;
        
        [NonSerialized] private List<int> CookingLevelRequirements = new() { 1, 5, 15, 30, 60, 100, 150, 200 };
        Wallet Wallet => Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet;

        public void OnAfterLoad()
        {
            if (CookingRecords == null)
            {
                CookingRecords = new CookingRecord[4].ToList();
            }
        }
        
        public bool CanUseSlot(int slot)
        {
            var skillInfo = Services.Container.Resolve<SkillService>().GetPlayerSkillInfo(SkillData.Type.Cooking);
            var levelRequirement = CookingLevelRequirements[slot];
            return skillInfo.Level >= levelRequirement;
        }

        private CookingRecipe GetRecipe(Guid guid)
        {
            return Services.Container.Resolve<NodeDatabaseService>().NodeDatabase.GetNode(guid) as CookingRecipe;
        }

        public float SecondsToCook(CookingRecipe recipe)
        {
            var cookingProgressParameters = Services.Container.Resolve<SkillService>().GetCombinedProgressParameters(SkillData.Type.Cooking, null);
            var totalProgressRequired = Mathf.CeilToInt(recipe.hp / (float)cookingProgressParameters.BasePower);
            return 1f / cookingProgressParameters.ActionFreqHz * totalProgressRequired;
        }

        public CookingSummary GetCookingSummary(int slot)
        {
            if (CookingRecords == null || CookingRecords.Count <= slot)
                return null;

            var cookingRecord = CookingRecords[slot];
            if (cookingRecord == null)
                return null;

            long maxTime = 3600;
            var timeDelta = OfflineTracker.TimeNowUnix - cookingRecord.startTime;
            timeDelta = Math.Min(maxTime, timeDelta);
            var recipe = GetRecipe(cookingRecord.recipeId);
            var timeToCook = SecondsToCook(recipe);

            long numberOfCookedItems = Mathf.FloorToInt(timeDelta / timeToCook);

            foreach (var input in recipe.Inputs)
            {
                var itemAmount = Wallet.ItemAmount(input.item.Uuid);
                var costPerCookedItem = input.quantity;

                var maxInputsThatCanBeUsed = itemAmount / costPerCookedItem;
                numberOfCookedItems = Math.Min(numberOfCookedItems, maxInputsThatCanBeUsed);
            }

            var ins = new Dictionary<Guid, long>();
            foreach (var itemWithQuantity in recipe.Inputs)
            {
                ins.Add(itemWithQuantity.item.Uuid, itemWithQuantity.quantity * numberOfCookedItems);
            }

            var outs = new Dictionary<Guid, long>();
            foreach (var itemWithQuantity in recipe.Output)
            {
                outs.Add(itemWithQuantity.item.Uuid, itemWithQuantity.quantity * numberOfCookedItems);
            }

            return new CookingSummary
            {
                Ins = ins,
                Outs = outs,
                TotalCookedItems = numberOfCookedItems,
                LastCookStartTime = 0,
            };
        }
        
        public Dictionary<Guid, long> Collect(int slot)
        {
            var summary = GetCookingSummary(slot);

            if (summary == null || summary.TotalCookedItems == 0)
                return null;

            if (!Wallet.SpendItems(summary.Ins)) 
                return null;

            Wallet.GiveItems(summary.Outs);
            CookingRecords[slot].startTime = OfflineTracker.TimeNowUnix - summary.LastCookStartTime;
            Services.Container.Resolve<InventoryService>().Save();
            
            return summary.Outs;
        }

        public void SetRecipe(Guid recipe, int slot)
        {
            if (!CanUseSlot(slot))
                return;
            Collect(slot);
            CookingRecords[slot] = new CookingRecord
            {
                recipeId = recipe,
                startTime = OfflineTracker.TimeNowUnix,
            };
            
            Services.Container.Resolve<InventoryService>().Save();
        }
    }
    [Serializable]
    public class CookingRecord
    {
        public long startTime;
        public Guid recipeId;
    }
}