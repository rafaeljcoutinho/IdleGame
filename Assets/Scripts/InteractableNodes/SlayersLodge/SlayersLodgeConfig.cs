using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slayer
{
    [CreateAssetMenu(menuName = "SlayersLodgeData", fileName = "Slayer/SlayersLodge", order = 0)]
    public class SlayersLodgeConfig : NodeData
    {
        public ActionNodeData enemy;
        public SlayerLodgeUpgrade unlock;
        public List<SlayerLodgeUpgrade> UpgradeConfigs;

        public int MaxLevel => UpgradeConfigs.Count;
        
        [Serializable]
        public class SlayerLodgeUpgrade
        {
            public bool showUI;
            public List<ItemWithQuantity> Costs;
            public List<ItemWithQuantity> Rewards;
            public ConditionalModifier modifiers;

            public DropTableUnlockConfig dropTableUnlock;
            public int MaxKillStreakIncrease;
        }

        [Serializable]
        public class DropTableUnlockConfig
        {
            public Droptable droptable;
            public Item item;
            public float chance;

            public DropTableUnlock ToProgressionData()
            {
                return new DropTableUnlock
                {
                    droptableId = Guid.Parse(droptable.id),
                    itemId = item.Uuid,
                    chance = chance,
                };
            }
        }

        [Serializable]
        public class ConditionalModifier
        {
            public ActionModifiers modifier;
            public bool global;
            public List<ActionNodeData> affectedNodes;

            public bool ContainsNode(Guid id)
            {
                foreach (var node in affectedNodes)
                {
                    if (node.Uuid == id)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        
        [Serializable]
        public class DropTableUnlock
        {
            public Guid droptableId;
            public Guid itemId;
            public float chance;
            
            public Droptable.Drop ToDroptableDrop()
            {
                return new Droptable.Drop
                {
                    item = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(itemId),
                    minAmount = 1,
                    maxAmount = 1,
                    percentage = chance,
                };
            }
        }
    }
}