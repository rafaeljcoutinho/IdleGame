using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDetailsView : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image questIcon;
    [SerializeField] private GameObject container;
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private TextItemList requirementsText;
    [SerializeField] private TextItemList rewardsText;
    [SerializeField] private Transform requirementsParent;
    [SerializeField] private Transform objectivesParent;
    [SerializeField] private Transform rewardsParent;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject backButtonLabel;
    [SerializeField] private Button trackButton;
    [SerializeField] private Button acceptQuest;
    [SerializeField] private TextMeshProUGUI acceptLabel;
    [SerializeField] private TextMeshProUGUI inProgress;
    [SerializeField] private TextMeshProUGUI trackLabel;
    [SerializeField] private ScrollRect scrollRect;

    private QuestTrackerManager trackerManager;
    private Guid currentQuestId;
    public Guid CurrentQuestId => currentQuestId;

    private List<GameObject> allPrefabs = new();

    private const string LEVEL_REQUIREMENT = "Level {0} {1}";
    private const string QUEST_REQUIREMENT = "Completed quest: {0}";
    private const string OBJECTIVES_TEXT_HAVE = "Collect <color={0}><u>{1}</u></color>: {2}/{3}";
    private const string COMPLETE_TEXT = "<s> {0} </s>";

    private const string TALK_TO_TEXT = "Talk to {0}";

    private const string REWARDS_TEXT = "{0} <color={1}><u>{2}</u></color>";

    private const string GET_QUEST_TITLE_LOC = "quests.{0}.title";
    private const string CAN_START_TEXT = "<color=#33AAAA>You have all the requirements to start the quest!";

    private Action startCallback;
    public void OpenDetails(Guid questId, Action trackCallback, Action startCallback, QuestTrackerManager questTracker, bool showBackButton)
    {
        foreach (var go in allPrefabs)
        {
            Destroy(go);
        }
        allPrefabs.Clear();
        trackButton.onClick.RemoveAllListeners();
        acceptQuest.onClick.RemoveAllListeners();

        trackerManager = questTracker;
        currentQuestId = questId;
        trackButton.onClick.AddListener(()=> trackCallback());
        this.startCallback = startCallback;
        backButtonLabel.SetActive(showBackButton);
        Init();
        scrollRect.normalizedPosition = new Vector2(0f, 1f);
        gameObject.SetActive(true);
        acceptQuest.onClick.AddListener(StartQuestClick);
    }

    private void StartQuestClick()
    {
        var qpc = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        var questDataBase = Services.Container.Resolve<QuestDatabaseService>().QuestDatabase;
        var currentQuestData = questDataBase.GetQuest(currentQuestId);
        var ok = qpc.StartQuest(currentQuestData);
        if (ok)
        {
            startCallback?.Invoke();
            Refresh();
            trackButton.onClick?.Invoke();
        }
        gameObject.SetActive(false);
    }

    public void CloseDetails()
    {
        currentQuestId = Guid.Empty;
        startCallback = null;
        gameObject.SetActive(false);
        foreach (var go in allPrefabs)
        {
            Destroy(go);
        }
        allPrefabs.Clear();
        trackButton.onClick.RemoveAllListeners();
        acceptQuest.onClick.RemoveAllListeners();
    }

    private void Init()
    {
        var questsProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        var questDataBase = Services.Container.Resolve<QuestDatabaseService>().QuestDatabase;

        var currentQuestData = questDataBase.GetQuest(currentQuestId);
        
        var skillServices = Services.Container.Resolve<SkillService>();
        var questRequirements = currentQuestData.RequirementsToStart;
        
        var currentQuestProgress = questsProgressContainer.GetQuestProgress(currentQuestData);

        var unlocked = currentQuestProgress != null;
        var canStart = questsProgressContainer.CanStart(currentQuestData);
        var started = unlocked && currentQuestProgress.Started;
        var completed = started && currentQuestProgress.Completed;

        //var currentStep = currentQuestProgress.CurrentInprogressStep();
        
        var wallet = Services.Container.Resolve<Wallet>();
        
        questIcon.sprite = currentQuestData.questIcon;
        questName.text = Services.Container.Resolve<LocalizationService>().LocalizeText(currentQuestData.LocalizationKey);

        if (completed)
        {
            trackLabel.gameObject.SetActive(false);
            inProgress.text = "COMPLETED";
            inProgress.gameObject.SetActive(true);
        }
        else if (started)
        {
            trackLabel.gameObject.SetActive(true);
            inProgress.text = "IN PROGRESS";
            inProgress.gameObject.SetActive(true);
        }
        else
        {
            trackLabel.gameObject.SetActive(false);
            inProgress.gameObject.SetActive(false);
        }

        var showQuestRequirements = false;
        foreach (var levelRequirement in questRequirements.SkillLevels)
        {
            showQuestRequirements = true;
            var currentSkillProgress = skillServices.GetPlayerSkillInfo(levelRequirement.skill).Level;

            var complete = levelRequirement.level <= currentSkillProgress;

            var go = Instantiate(requirementsText, requirementsParent);
            allPrefabs.Add(go.gameObject);

            var text = string.Format(LEVEL_REQUIREMENT, levelRequirement.level, levelRequirement.skill);
            if (complete) text = string.Format(COMPLETE_TEXT, text);

            go.SetText(text);
        }

        foreach (var questRequirement in questRequirements.CompletedQuests)
        {
            showQuestRequirements = true;
            var complete = questsProgressContainer.GetQuestProgress(questRequirement).Completed;
            var go = Instantiate(requirementsText, requirementsParent);

            allPrefabs.Add(go.gameObject);
            
            var text = string.Format(QUEST_REQUIREMENT, questRequirement.name);
            if (complete) text = string.Format(COMPLETE_TEXT, text);

            go.SetText(text);
        }
        if (completed)
        {
            showQuestRequirements = false;
        }
        requirementsParent.gameObject.SetActive(showQuestRequirements);

        if (!started || completed)
        {
            objectivesParent.gameObject.SetActive(false);
            container.SetActive(showQuestRequirements);
        }
        else 
        {
            objectivesParent.gameObject.SetActive(true);
        }

        if (started && !completed)
        {
            var currentStep = currentQuestProgress.CurrentInprogressStep();
            if (currentQuestProgress.StepProgress[currentStep].finishedDialog)
            {
                var i = 0;
                foreach (var objective in currentQuestData.Steps[currentStep].Objectives)
                {
                    //OBJECTIVES TYPE_HAVE
                    var walletAmount = wallet.ItemAmount(objective.item.Uuid);
                    var complete = objective.amount <= wallet.ItemAmount(objective.item.Uuid);
                    var go = Instantiate(requirementsText, objectivesParent);
                    allPrefabs.Add(go.gameObject);
                    go.SetButtonAction( ()=> ShowTooltip(objective.item, go.transform.position));

                    var quantityFormated = SkillExperienceTable.Format(objective.amount);
                    var currentAmountFormated = SkillExperienceTable.Format(walletAmount);
                    var itemRarityColor = GetRarityColorHexa(objective.item);
                    
                    var text = string.Format
                    (
                        OBJECTIVES_TEXT_HAVE,
                        itemRarityColor,
                        objective.item.LocalizationKey, 
                        currentAmountFormated, 
                        quantityFormated
                    );
                    if (complete) text = string.Format(COMPLETE_TEXT, text);

                    go.SetText(text);
                    i++;
                }
                if (i == 0) objectivesParent.gameObject.SetActive(false);
            }

            if (IsTalkNeeded())
            {
                var npcName = TempLocalizations.Texts[currentQuestData.Steps[currentStep].npc.npcNameKey];
                var go = Instantiate(requirementsText, objectivesParent);
                allPrefabs.Add(go.gameObject);
                go.SetText(string.Format(TALK_TO_TEXT, npcName));
            }
        }
        
        foreach (var reward in currentQuestData.Rewards)
        {
            var go = Instantiate(rewardsText, rewardsParent);
            allPrefabs.Add(go.gameObject);
            var rarityItemColor = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.NameColorHexa[reward.item.rarity];
            var quantityFormated = SkillExperienceTable.Format(reward.quantity);
            go.SetText(string.Format(REWARDS_TEXT, quantityFormated, rarityItemColor, reward.item.LocalizationKey));
            var item = reward.item;
            go.SetButtonAction( ()=> ShowTooltip(item, go.transform.position));
        }

        if (currentQuestProgress != null && currentQuestProgress.Started)
        {
            inProgress.gameObject.SetActive(true);
            if (currentQuestId == trackerManager.TrackingQuest || trackerManager.TrackingQuest == Guid.Empty)
            {
                trackLabel.text = "TRACKED";
            }
            else 
            {
                trackLabel.text = "TRACK";
            }
        }
        else 
        {
            inProgress.gameObject.SetActive(false);
        }

        if (!started && canStart)
        {
            acceptLabel.gameObject.SetActive(true);
            var go = Instantiate(requirementsText, requirementsParent);
            go.SetText(CAN_START_TEXT);
            allPrefabs.Add(go.gameObject);

            acceptQuest.interactable = true;
        }
        else
        {
            acceptLabel.gameObject.SetActive(false);
        }
    }

    private bool IsTalkNeeded()
    {
        var questsProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        var questDataBase = Services.Container.Resolve<QuestDatabaseService>().QuestDatabase;

        var currentQuestData = questDataBase.GetQuest(currentQuestId);
        var currentQuestProgress = questsProgressContainer.GetQuestProgress(currentQuestData);
        var currentStepIndex = currentQuestProgress.CurrentInprogressStep();
        var currentStep = currentQuestData.Steps[currentStepIndex];
        var currentProgress = questsProgressContainer.ProgressPerQuest[Guid.Parse(currentQuestData.id)].StepProgress[currentStepIndex];

        var wallet = Services.Container.Resolve<Wallet>();
        int completeQuantity = 0;
        if (currentProgress.finishedDialog)
        {
            for (int i = 0; i < currentStep.Objectives.Count; i++)
            {
                var complete = currentStep.Objectives[i].amount <= wallet.ItemAmount(currentStep.Objectives[i].item.Uuid);

                if (complete) completeQuantity++;
            }
        }
        if (completeQuantity == currentStep.Objectives.Count || !currentProgress.finishedDialog)
        {
            return true;
        }
        return false;
    }
    
    private void ShowTooltip(Item item, Vector3 pos)
    {
        OverlayCanvas.Instance.ToolTip.Show(item, pos, ConsumableItemResolver.Source.Inventory, true);
    }

    private string GetRarityColorHexa(Item item)
    {
        return Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.RarityColorHexa[item.rarity];
    }

    public void Refresh()
    {
        foreach (var go in allPrefabs)
        {
            Destroy(go);
        }
        allPrefabs.Clear();
        Init();
    }

}
