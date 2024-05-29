using System;
using System.Collections.Generic;
using System.Linq;
using Slayer;
using UnityEngine;
using Random = UnityEngine.Random;


public class SkillService
{
    private const float minGlobalAccuracy = .05f;
    
    public class ActionProgressResult
    {
        public bool Hit;
        public int Progress;
        public bool IsCritical;
        public float Cooldown;

        public OverkillInfo overkillInfo;
    }

    public class OverkillInfo
    {
        public float OverkillAmount;
        public bool IsOverkill;
        public float OverkillLogarithim;

        public static OverkillInfo NoOverkill = new OverkillInfo
        {
            IsOverkill = false,
            OverkillLogarithim = 1,
            OverkillAmount = 1,
        };

    }
    
    private SkillDatabase skillDatabase;
    public SkillDatabase SkillDatabase => skillDatabase;

    public void Load(Action<bool> callback)
    {
        var loadOp = Resources.LoadAsync<SkillDatabase>("SkillDatabase");
        loadOp.completed += operation =>
        {
            if (operation.isDone)
            {
                skillDatabase = loadOp.asset as SkillDatabase;
            }
            skillDatabase.Start();
            callback?.Invoke(true);
        };

    }

    public SkillExperienceTable SkillExperienceTable { private set; get; }
    public event Action<List<LevelupEvent>> OnSkillLevelsChanged;

    private List<LevelupEvent> onLevelupArgsCache = new List<LevelupEvent>(3);
    public struct LevelupEvent
    {
        public SkillData.Type skill;
        public int oldLevel;
        public int newLevel;
    }
    
    public event Action<SkillData.Type> OnXpIncrease;
    private Dictionary<SkillData.Type, int> CurrentLevels;
    private Dictionary<Guid, SkillData> skillExpItems;

    public SkillService()
    {
        SkillExperienceTable = new SkillExperienceTable();
        
    }

    public void InitializeLevelUpBroker()
    {
        var wallet = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet;
        CurrentLevels = new Dictionary<SkillData.Type, int>();
        skillExpItems = new Dictionary<Guid, SkillData>();
        foreach (var skill in new List<SkillData.Type>
                 {
                     SkillData.Type.Woodcutting,
                     SkillData.Type.Fishing,
                     SkillData.Type.Melee,
                     SkillData.Type.Cooking,
                     SkillData.Type.Character,
                 })
        {
            var skillInfo = GetPlayerSkillInfo(skill);
            var skillXpItem = skillDatabase.GetSkillData(skill);
            skillExpItems.Add(skillXpItem.XpItem.Uuid, skillXpItem);
            CurrentLevels.Add(skill, skillInfo.Level);
        }
        wallet.OnItemAmountChanged += WalletOnOnItemAmountChanged;
    }

    private void WalletOnOnItemAmountChanged(Dictionary<Guid, long> itemsChanged)
    {
        onLevelupArgsCache.Clear();
        foreach (var kvPair in itemsChanged)
        {
            var itemId = kvPair.Key;
            if (skillExpItems.ContainsKey(itemId))
            {
                var skill = skillExpItems[itemId];
                // get player skill info is somewhat expensive. maybe improve it
                var currentSkillInfo = GetPlayerSkillInfo(skill.XpItem.Uuid);
                if (currentSkillInfo.Level != CurrentLevels[skill.SkillType])
                {
                    var oldLevel = CurrentLevels[skill.SkillType];
                    onLevelupArgsCache.Add(new LevelupEvent
                    {
                        skill = skill.SkillType,
                        oldLevel = oldLevel,
                        newLevel = currentSkillInfo.Level,
                    });
                    CurrentLevels[skill.SkillType] = currentSkillInfo.Level;
                }
                OnXpIncrease?.Invoke(skill.SkillType);
            }
        }

        if (onLevelupArgsCache.Count > 0)
        {
            OnSkillLevelsChanged?.Invoke(onLevelupArgsCache);
        }
    }


    public PlayerSkillInfo GetPlayerSkillInfo(SkillData.Type skill)
    {
        var skillXpItemId = skillDatabase.GetSkillData(skill).XpItem;
            var skillXp = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.ItemAmount(skillXpItemId.Uuid);
        var currentLevel = SkillExperienceTable.GetLevel(skillXp);
        
        return new PlayerSkillInfo
        {
            Level = currentLevel,
            XpInLevel = SkillExperienceTable.GetCurrentExperienceInLevel(skillXp),
            TotalXp = skillXp,
            TotalXpToNextLevel = SkillExperienceTable.GetExperienceToNextLevel(currentLevel),
        };
    }
    
    public PlayerSkillInfo GetPlayerSkillInfo(Guid skillExpItemId)
    {
        var skillXp = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.ItemAmount(skillExpItemId);
        var currentLevel = SkillExperienceTable.GetLevel(skillXp);
        
        return new PlayerSkillInfo
        {
            Level = currentLevel,
            XpInLevel = SkillExperienceTable.GetCurrentExperienceInLevel(skillXp),
            TotalXp = skillXp,
            TotalXpToNextLevel = SkillExperienceTable.GetExperienceToNextLevel(currentLevel),
        };
    }

    public (int , Dictionary<Guid, long>) GetAfkProgress(SkillData.Type skill, ActionNodeData nodeData, int timeInSeconds)
    {
        var kills = CalculateAfkKillCount(skill, nodeData, timeInSeconds);
        var dropGenerator = Services.Container.Resolve<DropGeneratorService>();
        return (kills, dropGenerator.GenerateDrops(nodeData.droptable, kills));
    }
    
    public long CalculateXpExpected(SkillData.Type skill, ActionNodeData nodeData, int timeInSeconds)
    {
        var kills = CalculateAfkKillCount(skill, nodeData, timeInSeconds);
        var dropGenerator = Services.Container.Resolve<DropGeneratorService>();
        var averageExp = dropGenerator.GetAverageExpDrop(skill, nodeData.droptable.GuaranteedDrops);
        return kills * averageExp;
    }

    public string GetAfkProgressTips(SkillData.Type skill, ActionNodeData nodeData, int timeInSeconds)
    {
        var combinedParams = GetCombinedProgressParameters(skill, nodeData);
        var totalProgress = Mathf.FloorToInt(Mathf.Clamp(combinedParams.BasePower - nodeData.flatResistance, 0, int.MaxValue) * (1-nodeData.percentResistence));
        if (totalProgress == 0)
        {
            return $"{nodeData.name} has too much resistence for you right now";
        }
        var rawDps = Mathf.Clamp01(combinedParams.CritChance) * totalProgress * combinedParams.CrititalDamage;
        var combinedSuccessProbability = combinedParams.SuccessChance * (1 - nodeData.dodge);
        if (combinedSuccessProbability < minGlobalAccuracy)
        {
            return $"You need more accuracy to get farm {nodeData.name}";
        }

        return null;
    }
    
    public int CalculateAfkKillCount(SkillData.Type skill, ActionNodeData nodeData, int timeInSeconds)
    {
        var combinedParams = GetCombinedProgressParameters(skill, nodeData);
        var totalProgress = Mathf.FloorToInt(Mathf.Clamp(combinedParams.BasePower - nodeData.flatResistance, 0, int.MaxValue) * (1-nodeData.percentResistence));
        var rawDps = totalProgress * (1 + Mathf.Clamp01(combinedParams.CritChance) * combinedParams.CrititalDamage);
        var combinedSuccessProbability = combinedParams.SuccessChance * (1 - nodeData.dodge);
        if (combinedSuccessProbability < minGlobalAccuracy)
        {
            return 0;
        }

        var damagePerAction = rawDps * Mathf.Clamp01(combinedSuccessProbability);
        if (damagePerAction == 0)
            return 0;
        
        var actionCooldown = 1f / combinedParams.ActionFreqHz;
        var respawnTime = nodeData.respawnCooldown * (1 - nodeData.chanceToAutoRespawn);
        var totalActionsToKill = nodeData.hp / damagePerAction;
        totalActionsToKill = Mathf.CeilToInt(totalActionsToKill);
        var totalSecondsToKill = totalActionsToKill * actionCooldown + respawnTime;
        var totalKills = Mathf.FloorToInt(timeInSeconds / totalSecondsToKill);
         return totalKills;
    }

    public (int,float) CalculateKillTime(int power, float critChance, float critDamage, float actionHz, float successChance, int hp, int flatResistance, float percentResistance, float dodge)
    {
        var totalProgress = Mathf.FloorToInt(Mathf.Clamp(power - flatResistance, 0, int.MaxValue) * (1-percentResistance));
        var rawDps = totalProgress * (1 + Mathf.Clamp01(critChance) * critDamage);
        var combinedSuccessProbability = successChance * (1 - dodge);
        if (combinedSuccessProbability < minGlobalAccuracy)
        {
            return (-1,0);
        }

        var damagePerAction = rawDps * Mathf.Clamp01(combinedSuccessProbability);
        if (damagePerAction == 0)
            return (-1,0);
        
        var actionCooldown = 1f / actionHz;
        var totalActionsToKill = Mathf.CeilToInt(hp / damagePerAction);
        var totalSecondsToKill = totalActionsToKill * actionCooldown;
        return (totalActionsToKill, totalSecondsToKill);
    }
    
    public (float,float) CalculateAfkKillCount_DEBUG(SkillData.Type skill, ActionNodeData nodeData, int timeInSeconds)
    {
        var combinedParams = GetCombinedProgressParameters(skill, nodeData);
        var totalProgress = Mathf.FloorToInt(Mathf.Clamp(combinedParams.BasePower - nodeData.flatResistance, 0, int.MaxValue) * (1-nodeData.percentResistence));
        var rawDps = totalProgress * (1 + Mathf.Clamp01(combinedParams.CritChance) * combinedParams.CrititalDamage);
        var combinedSuccessProbability = combinedParams.SuccessChance * (1 - nodeData.dodge);
        if (combinedSuccessProbability < minGlobalAccuracy)
        {
            return (0,0);
        }

        var damagePerAction = rawDps * Mathf.Clamp01(combinedSuccessProbability);
        if (damagePerAction == 0)
            return (0,0);
        
        var actionCooldown = 1f / combinedParams.ActionFreqHz;
        var respawnTime = nodeData.respawnCooldown * (1 - nodeData.chanceToAutoRespawn);
        var totalActionsToKill = nodeData.hp / damagePerAction;
        var totalSecondsToKill = totalActionsToKill * actionCooldown + respawnTime;
        return (combinedSuccessProbability, timeInSeconds / totalSecondsToKill);
    }

    public Dictionary<Guid, long> ScaleXpDrops(Dictionary<Guid, long> drops, ActionNodeData nodeData, SkillData.Type type)
    {
        var skillData = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(type);
        string itemID = skillData.XpItem.id;
        var equipedItems = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager
            .GetAllEquippedItems();
        ActionModifiers progressParams = GetCombinedModifiers(equipedItems, nodeData);
        foreach (var key in drops.Keys.ToList())
        {
            if (key.ToString() == itemID)
            {
                drops[key] = (long) (drops[key] * (1 + progressParams.GetValueForAttribute(skillData.bonusXp, 0)));
            }
        }

        return drops;
    }

    public ActionModifiers GetParametersForLevel(SkillData.Type skill, int level)
    {
        var skillData = skillDatabase.GetSkillData(skill);
        var levelModifiers = skillData.ModifiersForLevel(level);
        return levelModifiers;
    }

    public ActionProgressParameters GetCombinedProgressParameters(SkillData.Type skill, ActionNodeData node, params ActionModifiers[] otherModifiers)
    {
        var skillData = skillDatabase.GetSkillData(skill);
        var equipedItems = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.GetAllEquippedItems();
        var combinedModifiers = GetCombinedModifiers(equipedItems, node, otherModifiers);

        var level = GetPlayerSkillInfo(skill).Level;
        var levelModifiers = skillData.ModifiersForLevel(level);
        combinedModifiers.Combine(levelModifiers);

        // character bonus is 'global' affects all skills
        if (skill != SkillData.Type.Character)
        {
            var characterSkilldata = skillDatabase.GetSkillData(SkillData.Type.Character);
            var characterLevel = GetPlayerSkillInfo(SkillData.Type.Character);
            var playerLevelModifiers = characterSkilldata.ModifiersForLevel(characterLevel.Level);
            combinedModifiers.Combine(playerLevelModifiers);
        }

        var power = Mathf.FloorToInt(combinedModifiers.GetValueForAttribute(skillData.power, 0) * (1 + combinedModifiers.GetValueForAttribute(skillData.powerMulti, 0)));
        
        return new ActionProgressParameters
        {
            ActionFreqHz = combinedModifiers.GetValueForAttribute(skillData.speedMulti, .1f),
            SuccessChance = combinedModifiers.GetValueForAttribute(skillData.accu, 0),
            CritChance = combinedModifiers.GetValueForAttribute(skillData.critChance, 0),
            BasePower = power,
            CrititalDamage = combinedModifiers.GetValueForAttribute(skillData.critDamage, 0),
            Range = combinedModifiers.GetValueForAttribute(skillData.range, 0),
        };
    }
    
    
    public ActionModifiers GetCombinedModifiers(List<Item> items, ActionNodeData nodeData, params ActionModifiers[] otherModifiers)
    {
        var combinedModifiers = Services.Container.Resolve<BuffsManagerService>().GetCombinedBuffs();
        foreach (var item in items)
        {
            if (item is IEquipmentModifier equippedItem)
            {
                combinedModifiers.Combine(equippedItem.Modifiers);
            }
        }

        var nodeId = nodeData == null ? Guid.Empty : nodeData.Uuid;
        var slayerModifier = Services.Container.Resolve<InventoryService>().PlayerProfile.SlayerManager.GetAllCombinedModifiers(nodeId);
        combinedModifiers.Combine(slayerModifier);
        
        if (otherModifiers != null)
        {
            foreach (var modifier in otherModifiers)
            {
                combinedModifiers.Combine(modifier);
            }
        }
        return combinedModifiers;
    }

    public ActionProgressResult GetActionProgress(SkillData.Type skill, ActionNodeData actionNodeData, params ActionModifiers[] otherModifiers)
    {
        var combinedParams = GetCombinedProgressParameters(skill, actionNodeData, otherModifiers);

        var isCritical = Random.value < combinedParams.CritChance;
        var totalProgress = Mathf.Clamp(combinedParams.BasePower - actionNodeData.flatResistance, 0, int.MaxValue) * (1-actionNodeData.percentResistence);
        totalProgress *= isCritical ? (combinedParams.CrititalDamage + 1f) : 1f;
        totalProgress *= .9f + Random.value/5f; // between 90% and 110%

        var combinedSuccessProbability = 1f;
        if (actionNodeData.dodge > 0)
        {
            combinedSuccessProbability = combinedParams.SuccessChance / actionNodeData.dodge;
        }

        if (combinedSuccessProbability < minGlobalAccuracy)
        {
            return new ActionProgressResult
            {
                Hit = false,
                IsCritical = false,
                Progress = 0,
                Cooldown = 1f/combinedParams.ActionFreqHz,
                overkillInfo = OverkillInfo.NoOverkill,
            };
        }
    
        return new ActionProgressResult
        {
            Hit = Random.value < combinedSuccessProbability,
            IsCritical = isCritical,
            Progress = Mathf.RoundToInt(totalProgress),
            Cooldown = 1f/combinedParams.ActionFreqHz,
        };
    }

    public OverkillInfo GetOverkillInfo(int currentHp, int maxHp)
    {
        return OverkillInfo.NoOverkill;
    }
}