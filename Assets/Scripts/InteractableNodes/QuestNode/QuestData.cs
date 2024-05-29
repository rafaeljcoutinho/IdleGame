using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "QuestData", fileName = "Quest/Data", order = 0)]
public class QuestData : ScriptableObject
{
    [ScriptableObjectId] public string id;
    public Sprite questIcon;
    public List<QuestStep> Steps;
    public Requirement RequirementsToStart;
    public List<ItemWithQuantity> Rewards;
    public List<string> BarksForQuestStart;
    public Guid Uuid => Guid.Parse(id);
    public string LocalizationKey;
}

[Serializable]
public class Requirement
{
    public List<SkillRequirement> SkillLevels;
    public List<QuestData> CompletedQuests;

    public bool HasQuestRequirement(Guid id)
    {
        foreach (var quest in CompletedQuests)
        {
            if (quest.Uuid == id)
            {
                return true;
            }
        }

        return false;
    }
    
    [Serializable]
    public class SkillRequirement
    {
        public SkillData.Type skill;
        public int level;
    }

    public class RequirementsCheckResponse
    {
        public bool HasRequirements => HasLevelRequirements && HasQuestRequirements;
        public bool HasQuestRequirements;
        public bool HasLevelRequirements;
        public string LocalizedReasonForFail;
    }
}

[Serializable]
public class QuestStep
{
    public Npc npc;
    public List<QuestDialog> Dialogs;
    public List<QuestObjective> Objectives;
    public List<ItemWithQuantity> Rewards;
}

[Serializable]
public class QuestDialog
{
    public bool IsPlayerSpeaking;
    public string textKey;

    public List<ItemWithQuantity> Rewards;
}