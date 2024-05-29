using System;
using System.Collections.Generic;
using Cooking;
using Slayer;

[Serializable]
public class PlayerProfile
{
    public Wallet Wallet;
    public Wallet TransientWallet;
    public StoreManager storeManager;
    public TimeRewardManager timeRewardManager;
    public PlayerItemsTracker playerItemsTracker;
    public QuestProgressContainer QuestProgressContainer;
    public PlayerEquipmentManager PlayerEquipmentManager;
    public OfflineTracker OfflineTracker;
    public CookingManager CookingManager;
    public SlayerManager SlayerManager;
    public StatsCollector StatsCollector;
    public MapProgression MapProgression;

    [NonSerialized] private PlayerProfile Copy;

    public static PlayerProfile Default => new()
    {
        Wallet = Wallet.Default,
        QuestProgressContainer = QuestProgressContainer.Default,
        PlayerEquipmentManager = PlayerEquipmentManager.Default,
    };

    public void OnAfterLoad()
    {
        Wallet ??= Wallet.Default;
        TransientWallet ??= Wallet.Default;
        QuestProgressContainer ??= QuestProgressContainer.Default;
        PlayerEquipmentManager ??= PlayerEquipmentManager.Default;
        OfflineTracker ??= new OfflineTracker();
        CookingManager ??= new CookingManager();
        playerItemsTracker ??= new();
        storeManager ??= new StoreManager();
        timeRewardManager ??= new TimeRewardManager();
        SlayerManager ??= new SlayerManager();
        StatsCollector ??= new StatsCollector();
        MapProgression ??= new MapProgression(); 
        CookingManager.OnAfterLoad();
        SlayerManager.OnAfterLoad();
        StatsCollector.Init();
        QuestProgressContainer.Init();
        MapProgression.Init();
        storeManager.Init();
        playerItemsTracker.Init();
    }

    public List<SlayersLodgeConfig.DropTableUnlock> GetAllUnlockedDrops(Droptable droptable)
    {
        var ans = new List<SlayersLodgeConfig.DropTableUnlock>();
        ans.AddRange(SlayerManager.GetAllUnlocksForDroptable(droptable));
        return ans;
    }

    public Requirement.RequirementsCheckResponse MeetsRequirement(Requirement requirement)
    {
        // todo: dont allocate new one
        var ans = new Requirement.RequirementsCheckResponse()
        {
            HasLevelRequirements = true,
            HasQuestRequirements = true,
        };

        foreach (var quest in requirement.CompletedQuests)
        {
            var progress = QuestProgressContainer.GetQuestProgress(quest);
            if (progress == null || !progress.Completed)
            {
                ans.HasQuestRequirements = false;
                ans.LocalizedReasonForFail = $"You need to complete {quest.name} to do this";
                break;
            }
        }

        foreach (var skillRequirements in requirement.SkillLevels)
        {
            var skill = skillRequirements.skill;
            var level = skillRequirements.level;
            var expItem = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(skill).XpItem;
            var currentExp = Wallet.ItemAmount(expItem.Uuid);
            var currentLevel = Services.Container.Resolve<SkillService>().SkillExperienceTable
                .GetLevel(currentExp);
            if (currentLevel < level)
            {
                ans.HasLevelRequirements = false;
                ans.LocalizedReasonForFail = $"You need level {level} {skill.ToString()} to do this";
                break;
            }
        }
        return ans;
    }
}
