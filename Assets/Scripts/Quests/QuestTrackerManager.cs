
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class QuestTrackerManager : MonoBehaviour
{
    [SerializeField] private List<QuestObjectiveItemList> objectives;
    [SerializeField] private Transform questParent;

    private Guid trackingQuest = Guid.Empty;
    private const string HAVE_TITLE = "Collect {0}";
    private const string HAVE_PROGRESS = "{0}/{1}";
    private const string TALK_TO_TEXT = "Talk to {0}";
    private const string NewLine = "\n";

    public Guid TrackingQuest => trackingQuest;

    private void Start() {
        Init();
        var questProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        questProgressContainer.OnQuestStart += OnQuestStart;
        questProgressContainer.OnQuestCompleted += Refresh;
        questProgressContainer.OnQuestObjectiveUpdate += TrackQuest;
        questProgressContainer.OnQuestStepProgress += Refresh;
        questProgressContainer.OnQuestDialogFinish += OnQuestDialogFinish;
    }

    private void Refresh(Guid questId)
    {
        if (questId != trackingQuest) return;
        var delay = objectives[2].CompleteObjetive();
        DOVirtual.DelayedCall(delay, Reset);
    }

    public void SetTargetQuest(Guid questId)
    {
        ResetInsta();
        trackingQuest = questId;
        Init();
    }

    private void Reset()
    {
        float delay = 0;
        foreach (var obj in objectives)
        {
            delay = obj.FadeOut();
        }
        trackingQuest = Guid.Empty;

        DOVirtual.DelayedCall(delay, () => {
            foreach (var obj in objectives)
            {
                obj.gameObject.SetActive(false);
            }
            Init();
        });
    }

    private void ResetInsta()
    {
        foreach (var obj in objectives)
        {
            obj.Reset();
        }
        trackingQuest = Guid.Empty;
    }
    private void Init()
    {
        var questsProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;

        if (trackingQuest != Guid.Empty)
        {
            TrackQuest(trackingQuest);
            return;
        }

        foreach (var keyValuePair in questsProgressContainer.ProgressPerQuest)
        {
            if (keyValuePair.Value.Completed || questsProgressContainer.CanCompleteQuest(keyValuePair.Value)) 
                continue;
            if (keyValuePair.Value.Started)
            {
                TrackQuest(keyValuePair.Key);
                return;
            }
        }
        trackingQuest = Guid.Empty;
    }

    private void OnQuestStart(Guid questId)    
    {
        if(trackingQuest != Guid.Empty) return;

        TrackQuest(questId);
    }

    private void OnQuestDialogFinish(Guid questId)
    {
        if (questId != trackingQuest) return;

        var delay = objectives[2].CompleteObjetive();
        DOVirtual.DelayedCall(delay, () => {
            var delta = objectives[2].FadeOut();
            DOVirtual.DelayedCall(delta, () => {
                TrackQuest(questId);
            });
        });
    }

    public void TrackQuest(Guid questId)
    {
        trackingQuest = questId;

        var questsProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        var questData = Services.Container.Resolve<QuestDatabaseService>().QuestDatabase.GetQuest(questId);
        var currentStepIndex = questsProgressContainer.ProgressPerQuest[questId].CurrentInprogressStep();
        var currentStep = questData.Steps[currentStepIndex];
        var currentProgress = questsProgressContainer.ProgressPerQuest[Guid.Parse(questData.id)].StepProgress[currentStepIndex];

        int completeQuantity = 0;

        float delay = 0;
        if (currentProgress.finishedDialog)
        {
            for (int i = 0; i < currentStep.Objectives.Count; i++)
            {
                if (i >= 2) return;

                var objectiveData = GetObjectiveViewData(currentStep.Objectives[i], NewLine);
                if (objectiveData.isComplete) completeQuantity++;
                objectives[i].SetData(objectiveData);
                objectives[i].gameObject.SetActive(true);
                delay = objectives[i].FadeIn();
            }
        }
        if (completeQuantity == currentStep.Objectives.Count || !currentProgress.finishedDialog)
        {
            DOVirtual.DelayedCall(delay, () => {
                var npcName = TempLocalizations.Texts[currentStep.npc.npcNameKey];
                objectives[2].SetData(new ObjectiveViewData
                {
                    objectiveText = string.Format(TALK_TO_TEXT, npcName),
                    isComplete = false,
                }); 
                objectives[2].FadeIn();
            });
        }
        else
        {
            objectives[2].FadeOut();
        }

        return;
    }
    public static ObjectiveViewData GetObjectiveViewData(QuestObjective questObjective, string separator = "")
    {   
        if(questObjective.type == QuestObjective.Type.Have)
        {
            var objective = questObjective;
            var item = objective.item;
            var quantity = objective.amount;
            var currentAmount = Services.Container.Resolve<Wallet>().ItemAmount(item.Uuid);

            bool completed = currentAmount >= quantity;

            var quantityFormated = SkillExperienceTable.Format(quantity);
            var currentAmountFormated = SkillExperienceTable.Format(currentAmount);

            var newString = string.Format(HAVE_TITLE, item.LocalizationKey) + separator + string.Format(HAVE_PROGRESS, currentAmountFormated, quantityFormated);

            return new ObjectiveViewData
            {
                objectiveText = newString,
                isComplete = completed,
            };
        }
        else if(questObjective.type == QuestObjective.Type.Gather)
        {
            return new ObjectiveViewData{};
        }
        else if(questObjective.type == QuestObjective.Type.Kill)
        {
            return new ObjectiveViewData{};
        }
        else
        {
            return new ObjectiveViewData{};
        }
    }
}