using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllQuestView : MonoBehaviour
{
    [SerializeField] private QuestTrackerManager questTrackerManager;
    [SerializeField] private QuestDetailsView questDetailsView;
    [SerializeField] private QuestItemList questItemListPrefab;
    [SerializeField] private Transform inProgressParent;
    [SerializeField] private Transform completeParent;
    [SerializeField] private Image imageInProgress;
    [SerializeField] private Image imageComplete;
    [SerializeField] private Button collapseInProgress;
    [SerializeField] private Button collapseComplete;

    [SerializeField] private Transform listTransformParent;

    private Dictionary<Guid, QuestItemList> questGoList = new();

    private bool inProgressCollapsed = true;
    private bool completeCollapsed = true;

    private void OnEnable() 
    {
        Init();
        var questProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        questProgressContainer.OnQuestStart += Refresh;
        questProgressContainer.OnQuestCompleted += Refresh;
        questProgressContainer.OnQuestObjectiveUpdate += Refresh;
        questProgressContainer.OnQuestStepProgress += Refresh;
        questProgressContainer.OnQuestDialogFinish += Refresh;

    }

    private void Refresh(Guid questId)
    {
        if(questDetailsView.gameObject.activeInHierarchy && questDetailsView.CurrentQuestId == questId)
            questDetailsView.Refresh();
    }

    private void OnDisable() 
    {
        foreach(var go in questGoList)
        {
            Destroy(go.Value.gameObject);
        }
        questGoList.Clear();

        var questProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        questProgressContainer.OnQuestStart -= Refresh;
        questProgressContainer.OnQuestCompleted -= Refresh;
        questProgressContainer.OnQuestObjectiveUpdate -= Refresh;
        questProgressContainer.OnQuestStepProgress -= Refresh;
        questProgressContainer.OnQuestDialogFinish -= Refresh;
    }

    private void LateUpdate() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    private void Start() 
    {
        collapseInProgress.onClick.AddListener(OnCollapseInProgress);
        collapseComplete.onClick.AddListener(OnCollapseComplete);
    }

    private void Init()
    {
        Transform parent;
        var questsProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        var questDataBase = Services.Container.Resolve<QuestDatabaseService>().QuestDatabase;

        var allQuests = questDataBase.AllQuests;
        bool isEnable;

        foreach (var quest in allQuests)
        {
            var questId = Guid.Parse(quest.id);

            if (!questsProgressContainer.ProgressPerQuest.ContainsKey(questId))
            {
                continue;
            }
            
            if (questsProgressContainer.ProgressPerQuest[questId].Completed)
            {
                isEnable = !completeCollapsed;
                parent = completeParent;
            }
            else
            {
                isEnable = !inProgressCollapsed;
                parent = inProgressParent;          
            }
            var go = Instantiate(questItemListPrefab, parent);
            go.gameObject.SetActive(isEnable);
            go.SetQuestName(Services.Container.Resolve<LocalizationService>().LocalizeText(quest.LocalizationKey));
            
            go.SetButton(()=> OpenQuestDetails(questId, null, true));    
            questGoList.Add(questId, go); 
        }
    }

    private void OnCollapseComplete()
    {
        completeCollapsed = !completeCollapsed;
        var questsProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        foreach(var keyValuePair in questGoList)
        {
            if (questsProgressContainer.ProgressPerQuest[keyValuePair.Key].Completed)
            {
                keyValuePair.Value.gameObject.SetActive(!completeCollapsed);
            }
        }
        imageComplete.transform.Rotate(0, 0, imageComplete.transform.rotation.z + 180);
    }

    private void OnCollapseInProgress()
    {
        inProgressCollapsed = !inProgressCollapsed;
        var questsProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        foreach(var keyValuePair in questGoList)
        {
            if (!questsProgressContainer.ProgressPerQuest[keyValuePair.Key].Completed)
            {
                keyValuePair.Value.gameObject.SetActive(!inProgressCollapsed);
            }
        }
        imageInProgress.transform.Rotate(0, 0, imageInProgress.transform.rotation.z + 180);
    }

    public void Show()
    {
        questDetailsView.CloseDetails();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OpenQuestDetails(Guid questId, Action OnStart, bool showBackButton = false)
    {
        questDetailsView.OpenDetails
        (
            questId,
            () =>
            {
                questTrackerManager.SetTargetQuest(questId);
                questDetailsView.Refresh();
            },
            OnStart,
            questTrackerManager,
            showBackButton
        );
    }

}
