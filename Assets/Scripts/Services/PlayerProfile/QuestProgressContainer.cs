using System;
using System.Collections.Generic;

[Serializable]
public class QuestProgressContainer
{
    public Dictionary<Guid, QuestProgress> ProgressPerQuest;

    [NonSerialized]public Action<Guid> OnQuestStart;
    [NonSerialized]public Action<Guid> OnQuestStepProgress;
    [NonSerialized]public Action<Guid> OnQuestDialogProgress;
    [NonSerialized]public Action<Guid> OnQuestDialogFinish;
    [NonSerialized]public Action<Guid> OnQuestCompleted;
    [NonSerialized]public Action<Guid> OnQuestObjectiveUpdate;


    public void OnOnWalletAmountChange(Dictionary<Guid, long> changedItems) 
    { 
        foreach (var keyValuePair in ProgressPerQuest)
        {
            if (keyValuePair.Value.Completed || CanCompleteQuest(keyValuePair.Value)) 
                continue;

            if (!keyValuePair.Value.Started) 
                continue;

            var questData = Services.Container.Resolve<QuestDatabaseService>().QuestDatabase.GetQuest(keyValuePair.Key);
            var currentStepIndex = keyValuePair.Value.CurrentInprogressStep();

            if (keyValuePair.Value.StepProgress[currentStepIndex].Completed) continue;

            if (!keyValuePair.Value.StepProgress[currentStepIndex].finishedDialog) continue;

            foreach(var obj in questData.Steps[currentStepIndex].Objectives)
            {
                if(changedItems.ContainsKey(obj.item.Uuid))
                {
                    OnQuestObjectiveUpdate?.Invoke(Guid.Parse(questData.id));
                    break;
                }
            }
            
        }
    }

    public void Init() {
        Services.Container.Resolve<Wallet>().OnItemAmountChanged += OnOnWalletAmountChange;
        var overkillService = Services.Container.Resolve<OverkillService>();
        overkillService.OnNodeDeath += OnNodeDeath;
    }

    public void OnNodeDeath(NodeData node, int count)
    {
        Save();
    }

    public static QuestProgressContainer Default => new QuestProgressContainer
    {
        ProgressPerQuest = new(),
    };

    [Serializable]
    public class QuestProgress
    {
        public bool Started;
        public bool Completed;
        public List<QuestStepProgress> StepProgress;

        public QuestProgress()
        {
        }
        
        public QuestProgress(QuestData questData)
        {
            StepProgress = new List<QuestStepProgress> ();
            var i = 0;
            foreach (var questStep in questData.Steps)
            {
                StepProgress.Add(new QuestStepProgress(i, questStep));
                i++;
            }
            Completed = false;
            Started = false;
        }

        public int CurrentInprogressStep()
        {
            foreach (var step in StepProgress)
            {
                if (!step.Completed)
                    return step.Index;
            }

            return -1;
        }
    }
    
    [Serializable]
    public class QuestStepProgress
    {
        public int Index;
        public bool Completed;
        public bool Started;
        public int currentDialog;

        public bool finishedDialog;
        public Dictionary<string, int> requirementsProgress;

        public QuestStepProgress(int index, QuestStep step)
        {
            Index = index;
            requirementsProgress = new Dictionary<string, int>();
        }
    }

    [Serializable]
    public class QuestStepRequirementProgress
    {
        public Guid id;
        public int amount;
    }

    public bool InitQuestUnlocking(QuestData questData)
    {
        if (ProgressPerQuest.ContainsKey(questData.Uuid))
        {
            return false;
        }

        ProgressPerQuest.Add(questData.Uuid, new QuestProgress(questData));
        return true;
    }
    
    public bool StartQuest(QuestData questData)
    {
        if (!CanStart(questData))
        {
            return false;
        }
        if (!ProgressPerQuest.ContainsKey(questData.Uuid)) 
            return false;

        var questProgress = GetQuestProgress(questData);
        questProgress.Started = true;

        Save();
        OnQuestStart?.Invoke(questData.Uuid);
        return true;
    }

    public bool CanCompleteStep(QuestData questData, int stepId)
    {
        var progress = GetQuestProgress(questData);
        if (progress.Completed)
        {
            return false;
        }
        
        if (stepId != progress.CurrentInprogressStep())
        {
            return false;
        }

        var progressStep = progress.StepProgress[stepId];
        if (progressStep.Completed)
        {
            return false;
        }

        progressStep.finishedDialog = progressStep.currentDialog >= questData.Steps[stepId].Dialogs.Count;

        if (!progressStep.finishedDialog)
        {
            return false;
        }

        return CheckStepRequirements(questData.Steps[stepId]);
    }

    public bool CheckStepRequirements(QuestStep stepData)
    {
        foreach (var objective in stepData.Objectives)
        {
            switch (objective.type)
            {
                case QuestObjective.Type.Have:
                    var targetAmount = objective.amount;
                    var item = objective.item;
                    var currentAmount = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.ItemAmount(item.Uuid);
                    if (currentAmount < targetAmount)
                        return false;
                    break;
            }
        }

        return true;
    }

    public void CompleteStep(QuestData questData, int step)
    {
        if (!CanCompleteStep(questData, step))
            return;

        var objectives = questData.Steps[step].Objectives;
        var rewards = questData.Steps[step].Rewards;
        var inventory = Services.Container.Resolve<InventoryService>();


        var progress = GetQuestProgress(questData);
        progress.StepProgress[step].Completed = true;
        OnQuestStepProgress?.Invoke(questData.Uuid);

        foreach (var objective in objectives)
        {
            switch (objective.type)
            {
                case QuestObjective.Type.Have:
                    inventory.PlayerProfile.Wallet.SpendItem(objective.item.Uuid, objective.amount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        ApplyRewards(rewards);
        Save();
    }

    public bool CompleteQuest(QuestData questData)
    {
        var progress = GetQuestProgress(questData);
        if (CanCompleteQuest(progress))
        {
            progress.Completed = true;
            var rewards = questData.Rewards;
            ApplyRewards(rewards);
            OnQuestCompleted?.Invoke(questData.Uuid);
            Save();
            return true;
        }
        return false;
    }

    public bool CanCompleteQuest(QuestProgress progress)
    {
        if (progress == null || progress.Completed)
        {
            return false;
        }
        foreach (var steps in progress.StepProgress)
        {
            if (!steps.Completed)
                return false;
        }
        
        return true;
    }

    void ApplyRewards(List<ItemWithQuantity> rewards)
    {
        var itemsToGive = new Dictionary<Guid, long>();
        foreach (var reward in rewards)
        {
            itemsToGive.Add(reward.item.Uuid, reward.quantity);
        }
        var inventory = Services.Container.Resolve<InventoryService>();
        inventory.PlayerProfile.Wallet.GiveItems(itemsToGive);
        OverlayCanvas.Instance.ShowDrops(itemsToGive);
    }
    
    public void CompleteDialog(QuestData questData, int step, int dialog)
    {
        var progress = GetQuestProgress(questData);
        var progressStep = progress.StepProgress[step];
        ApplyRewards(questData.Steps[step].Dialogs[dialog].Rewards);
        progress.StepProgress[step].currentDialog = dialog + 1;
        progressStep.finishedDialog = progressStep.currentDialog >= questData.Steps[step].Dialogs.Count;
        if (progressStep.finishedDialog) 
        {
            OnQuestDialogFinish?.Invoke(questData.Uuid);
        }
        OnQuestDialogProgress?.Invoke(questData.Uuid);
        Save();
    }

    void Save()
    {
        Services.Container.Resolve<InventoryService>().Save();
    }

    public bool CanStart(QuestData questData)
    {
        var canStart = Services.Container.Resolve<InventoryService>().PlayerProfile
            .MeetsRequirement(questData.RequirementsToStart);
        return canStart.HasRequirements;
    }
    
    public QuestProgress GetQuestProgress(QuestData questData)
    {
        var id = Guid.Parse(questData.id);
        if (!ProgressPerQuest.ContainsKey(id))
        {
            return null;
        }
        return ProgressPerQuest[id];
    }
}