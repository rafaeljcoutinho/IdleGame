using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slayer
{
    [Serializable]
    public class SlayersLodgeProgression
    {
        public Guid lodgeId;
        public int level;
        public bool unlocked;
        public int maxKillStreak;
        public List<SlayersLodgeConfig.DropTableUnlock> unlockedDrops;
        public List<int> unlockedUpgrades;

        public bool IsUpgradeUnlocked(int level)
        {
            return unlocked && unlockedUpgrades.Contains(level);
        }
    }

    [Serializable]
    public class SlayerManager
    {
        private const int DEFAULT_MAX_KILLSTREAK = 1;
        public Dictionary<Guid, SlayersLodgeProgression> Progression;
        public List<Guid> Chests;

        [NonSerialized]public Action OnNewChest;
        private const int chestCapacity = 10;
        
        public void OnAfterLoad()
        {
            if (Progression == null)
            {
                Progression = new Dictionary<Guid, SlayersLodgeProgression>();
            }

            if (Chests == null)
            {
                Chests = new List<Guid>();
            }
        }

        public bool CanGetMoreChests()
        {
            return Chests.Count < chestCapacity;
        }
        
        public void GiveChest(Guid id)
        {
            return;
            
            if (CanGetMoreChests())
            {
                Chests.Add(id);
                OnNewChest?.Invoke();
            }
        }
        
        public Dictionary<Guid, long> OpenChests()
        {
            var itemDrops = new Dictionary<Guid, long>();
            foreach (var id in Chests)
            {
                var droptable = Services.Container.Resolve<DroptableDatabaseService>().DropTableDatabase.GetDroptable(id);
                var drops = Services.Container.Resolve<DropGeneratorService>().GenerateDrops(droptable, 1);
                itemDrops = Utils.Combine(itemDrops, drops);
            }
            Chests.Clear();
            return itemDrops;
        }

        public void PlayerDeath()
        {
            Chests.Clear();
        }

        public List<SlayersLodgeConfig.DropTableUnlock> GetAllUnlocksForDroptable(Droptable droptable)
        {
            var ans = new List<SlayersLodgeConfig.DropTableUnlock>();
            foreach (var kvPair in Progression)
            {
                foreach (var dropTableUnlock in kvPair.Value.unlockedDrops)
                {
                    if (Guid.Parse(droptable.id) == dropTableUnlock.droptableId)
                    {
                        ans.Add(dropTableUnlock);
                    }
                }
            }

            return ans;
        }

        public ActionModifiers GetAllCombinedModifiers(Guid node)
        {
            var ans = new ActionModifiers();
            foreach (var kvPair in Progression)
            {
                var config = GetConfig(kvPair.Key);
                var level = kvPair.Value.level;
                var unlockedUpgrades = kvPair.Value.unlockedUpgrades;
                for (var i = 0; i < config.UpgradeConfigs.Count; i++)
                {
                    var upgrades = config.UpgradeConfigs[i];
                    // not unlocked
                    if (level < i)
                    {
                        break;
                    }

                    // not unlocked upgrade
                    if (!unlockedUpgrades.Contains(i))
                    {
                        break;
                    }

                    if (upgrades.modifiers != null)
                    {
                        var isGlobal = upgrades.modifiers.global;
                        if (isGlobal || upgrades.modifiers.ContainsNode(node))
                        {
                            ans.Combine(upgrades.modifiers.modifier);
                        }
                    }
                }
            }

            return ans;
        }

        private SlayersLodgeProgression CreateNewProgression(Guid lodgeId)
        {
            var newProgression = new SlayersLodgeProgression
            {
                lodgeId = lodgeId,
                level = 0,
                unlocked = false,
                unlockedUpgrades = new(),
                maxKillStreak = DEFAULT_MAX_KILLSTREAK,
                unlockedDrops = new List<SlayersLodgeConfig.DropTableUnlock>(),
            };

            Progression.Add(lodgeId, newProgression);
            return newProgression;
        }

        public SlayersLodgeProgression GetOrCreateProgression(Guid lodgeId)
        {
            if (Progression.ContainsKey(lodgeId))
                return Progression[lodgeId];
            return CreateNewProgression(lodgeId);
        }

        public bool IsMaxLevel(SlayersLodgeConfig config)
        {
            var lodgeProgression = Progression[config.Uuid];
            return lodgeProgression.level == config.MaxLevel;
        }

        public bool IsMaxLevel(Guid id)
        {
            var progression = GetOrCreateProgression(id);
            if (!progression.unlocked)
            {
                return false;
            }

            var config = GetConfig(id);
            return IsMaxLevel(config);
        }

        private SlayersLodgeConfig GetConfig(Guid id)
        {
            return Services.Container.Resolve<NodeDatabaseService>().NodeDatabase.GetNode(id) as SlayersLodgeConfig;
        }

        public bool CanUpgrade(Guid id)
        {
            var wallet = Services.Container.Resolve<Wallet>();
            var config = GetConfig(id);
            GetOrCreateProgression(id);
            var progression = Progression[id];

            if (IsMaxLevel(config))
            {
                Debug.LogWarning("Tried to update lodge already at max level");
            }

            var upgradeConfig = config.UpgradeConfigs[progression.level];
            if (!wallet.CanBuy(upgradeConfig.Costs.ToIdAmountDict()))
            {
                return false;
            }

            return true;
        }

        public bool Claim(Guid id, int level)
        {
            var progression = GetOrCreateProgression(id);
            if (progression.unlockedUpgrades.Contains(level))
            {
                return false;
            }

            if (progression.level < level)
            {
                Debug.LogError("Tried to claim a reward without the level requirement");
                return false;
            }

            var upgradeConfig = GetConfig(id).UpgradeConfigs[level];

            var rewards = upgradeConfig.Rewards.ToIdAmountDict();
            Services.Container.Resolve<Wallet>().GiveItems(rewards);

            progression.unlockedUpgrades.Add(level);
            progression.maxKillStreak += upgradeConfig.MaxKillStreakIncrease;
            if (upgradeConfig.dropTableUnlock.droptable != null)
            {
                progression.unlockedDrops.Add(upgradeConfig.dropTableUnlock.ToProgressionData());
            }
            
            Services.Container.Resolve<InventoryService>().Save();

            return true;
        }

        public bool Upgrade(Guid id)
        {
            if (!CanUpgrade(id))
            {
                return false;
            }

            var progression = Progression[id];

            var upgradeConfig = GetConfig(id).UpgradeConfigs[progression.level];
            var costs = upgradeConfig.Costs.ToIdAmountDict();
            var wallet = Services.Container.Resolve<Wallet>();
            var ok = wallet.SpendItems(costs);
            if (!ok)
            {
                Debug.LogError("Wallet transaction failed");
                return false;
            }

            progression.unlocked = true;
            progression.level++;
            Services.Container.Resolve<InventoryService>().Save();
            return true;
        }
    }
}