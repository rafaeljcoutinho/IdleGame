using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OfflineProgressChecker : MonoBehaviour
{
    private void Start()
    {
        var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
        var itemDatabase = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;
        var offlineProgression = profile.OfflineTracker.GetOfflineProgression();
        if (offlineProgression == null || offlineProgression.TimeInSeconds <= 5)
            return;

        if(profile.OfflineTracker.OfflineActivityRecord.skill is SkillData.Type.Melee or SkillData.Type.Ranged)
        {
            ConvertCombatExp(profile, offlineProgression);
        }
        profile.OfflineTracker.ResetOfflineActivity();

        profile.Wallet.AddPendingItems(Wallet.RewardSource.AfkGains, offlineProgression.Items);
        var time = TimeSpan.FromSeconds(offlineProgression.TimeInSeconds);
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.drops = itemDatabase.ToItemDict(offlineProgression.Items);
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.title = "AFK REWARDS";
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.subtitle = time.ToString(@"hh\:mm\:ss");
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.showVideoButton = true;
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.leftImage = offlineProgression.nodeIcon;
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.imageLabel = string.Format("x{0}", SkillExperienceTable.Format(offlineProgression.kills));
        OverlayCanvas.Instance.AfkProgressPopup.Show(_ =>
        {
            var rewards = profile.Wallet.ApplyPendingItems(Wallet.RewardSource.AfkGains);
            Services.Container.Resolve<InventoryService>().Save();            
            OverlayCanvas.Instance.ShowDrops(rewards);
            TryShowCompleteQuestPopup();
        });
        Services.Container.Resolve<InventoryService>().Save();
    }

    private List<Guid> questIds = new();

    private void TryShowCompleteQuestPopup()
    {
        var qpc = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        foreach (var kvPair in qpc.ProgressPerQuest)
        {
            var progress = kvPair.Value;
            if (qpc.CanCompleteQuest(progress))
            {
                questIds.Clear();
                var questData = Services.Container.Resolve<QuestDatabaseService>().QuestDatabase.GetQuest(kvPair.Key);
                questIds.Add(Guid.Parse(questData.id));
                OverlayCanvas.Instance.QuestCompleteViewController.Show(questIds);
            }
        }
    }

    private static void ConvertCombatExp(PlayerProfile profile, OfflineTracker.OfflineProgressResult offlineProgression)
    {
        var mainHandItem = profile.PlayerEquipmentManager.GetItemOnSlot(Equipment.EquipSlot.MainHand) as Equipment;
        var xpType = mainHandItem?.MainType ?? SkillData.Type.Melee;
        var xpItem = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(xpType).XpItem;
        var charXp = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(SkillData.Type.Character).XpItem;

        foreach (var itemId in offlineProgression.Items.Keys.ToList())
        {
            if (itemId == charXp.Uuid)
            {
                var xpAmount = offlineProgression.Items[itemId];
                offlineProgression.Items.Add(xpItem.Uuid, xpAmount);
            }
        }
    }
}