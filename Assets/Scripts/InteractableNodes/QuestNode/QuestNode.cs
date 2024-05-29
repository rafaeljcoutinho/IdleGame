using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestNode : BaseInteractableNodeMonobehaviour
{
    [SerializeField] private Npc npc;
    [SerializeField] private NodeData nodeData;
    [SerializeField] private Transform questIconAnchor;

    [Header("Baks")] 
    [SerializeField] private float checkInterval;

    public override NodeData NodeData => nodeData; 
    public Npc Npc => npc; 
    public override Type PlayerBehaviour => typeof(QuestTalkBehaviour);
    public RectTransform VisibilityContainer { get; set; }
    private QuestIconType questIconType;
    private float lastBarkTime = 0;

    private QuestData questData;

    private void Start()
    {
        var nPCService = Services.Container.Resolve<NPCService>();
        nPCService.OnNpcProgress += RefreshUI;
        VerifyCurrentQuestStep();
        if (questIconType == QuestIconType.None) return;
        UpdateCTA();
    }

    private int lastBark = 0;
    private void Update()
    {
        // skip 10 frames
        if (Time.frameCount % 10 == 3 && Time.time > lastBarkTime + checkInterval)
        {
            if (questIconType == QuestIconType.ExclamationMark && questData != null && questData.BarksForQuestStart.Count > 0)
            {
                var bark = questData.BarksForQuestStart[lastBark];
                lastBark++;
                lastBark %= questData.BarksForQuestStart.Count;
                var chs = Services.Container.Resolve<ContextualHintService>();
                if (chs.IsShowing())
                {
                    return;
                }
                chs.SetDisplayOptions(new ContextualHintService.HintDisplayOptions
                    {
                        offset = 2f,
                        anchorPosition = Contextual2DObject.AnchorPosition.Top,
                        fadeBackground = false,
                        dismissOnScreenInteraction = false,
                        autoHideSeconds = 4f,
                    })
                    .SetTargetTransform(transform)
                    .Show(new TextLayout.LayoutData {
                            LocalizedText = bark
                        }
                    );
            }

            lastBarkTime = Time.time;
        }
    }

    private void RefreshUI()
    {
        VerifyCurrentQuestStep();
        UpdateCTA();
    }

    private void UpdateCTA() 
    {
        OverlayCanvas.Instance.QuestCTAView.ShowCTA(npc.npcNameKey, questIconAnchor, questIconType);
    }

    private void VerifyCurrentQuestStep()
    {
        questData = null;
        questIconType = QuestIconType.None;
        if (!Services.Container.Resolve<NPCService>().NPCByQuests.ContainsKey(npc.npcNameKey)) return;

        var npcQuests = Services.Container.Resolve<NPCService>().NPCByQuests[npc.npcNameKey];
        var questProgress = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;

        foreach (var npcQuest in npcQuests)
        {
            if (!questProgress.ProgressPerQuest.ContainsKey(Guid.Parse(npcQuest.id)) || !questProgress.ProgressPerQuest[Guid.Parse(npcQuest.id)].Started)
            {
                questData = npcQuest;
                questIconType = QuestIconType.ExclamationMark;
                continue;
            }
            
            var currentQuestProgress = questProgress.ProgressPerQuest[Guid.Parse(npcQuest.id)];

            if (currentQuestProgress.Completed || questProgress.CanCompleteQuest(currentQuestProgress))
            {
                continue;
            }
             
            var currentStep = currentQuestProgress.CurrentInprogressStep();

            var objectives = npcQuest.Steps[currentStep].Objectives;

            int completeQuantity = 0;

            questIconType = QuestIconType.QuestionMark;
            for (int i = 0; i < objectives.Count; i++)
            {
                if (IsObjectiveCompleted(objectives[i])) completeQuantity++;
            }

            if (completeQuantity == objectives.Count)
            {
                questIconType = QuestIconType.CompletedQuestionMark;
                return;
            }
        }
    }
    private bool IsObjectiveCompleted(QuestObjective questObjective)
    {   
        if(questObjective.type == QuestObjective.Type.Have)
        {
            var item = questObjective.item;
            var quantity = questObjective.amount;
            var currentAmount = Services.Container.Resolve<Wallet>().ItemAmount(item.Uuid);

            bool completed = currentAmount >= quantity;

            return completed;
        }
        else if(questObjective.type == QuestObjective.Type.Gather)
        {
            return false;
        }
        else if(questObjective.type == QuestObjective.Type.Kill)
        {
            return false;
        }
        else
        {
            return false;
        }
    }
}

public class NPCService
{
    public Dictionary<string, List<QuestData>> NPCByQuests;

    public Action OnNpcProgress;

    public NPCService()
    {
        Init();
    }
    
    void Init()
    {
        NPCByQuests = new Dictionary<string, List<QuestData>>();
        PreComputeDict();
        var questProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        questProgressContainer.OnQuestDialogProgress += OnQuestDialogProgress;
        questProgressContainer.OnQuestCompleted += OnQuestDialogProgress;
        questProgressContainer.OnQuestStepProgress += OnQuestDialogProgress;
        questProgressContainer.OnQuestStart += OnQuestProgress;
        questProgressContainer.OnQuestDialogFinish += OnQuestProgress;
        questProgressContainer.OnQuestObjectiveUpdate += OnQuestProgress;
    }

    private void OnQuestProgress(Guid _)
    {
        OnNpcProgress?.Invoke();
    }

    private void OnQuestDialogProgress(Guid obj)
    {
        NPCByQuests.Clear();
        PreComputeDict();
        OnNpcProgress?.Invoke();
    }

    private void PreComputeDict()
    {
        var questDatabase = Services.Container.Resolve<QuestDatabaseService>().QuestDatabase;
        var questContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        foreach (var questData in questDatabase.AllQuests)
        {
            var progress = questContainer.GetQuestProgress(questData);
            var skip = (progress != null && progress.Completed) || (progress != null && questContainer.CanCompleteQuest(progress)); 
            if (skip)
            {
                continue;
            }
            var started = progress != null && progress.Started;
            if (started)
            {
                var currentStepIndex = progress.CurrentInprogressStep();
                if (progress.StepProgress.Count == currentStepIndex)
                {
                    continue;
                }
                var npc = questData.Steps[currentStepIndex].npc.npcNameKey;
                if (!NPCByQuests.ContainsKey(npc))
                {
                    NPCByQuests.Add(npc, new List<QuestData>());
                }

                NPCByQuests[npc].Add(questData);
            }
            else
            {
                var npc = questData.Steps[0].npc.npcNameKey;
                if (!NPCByQuests.ContainsKey(npc))
                {
                    NPCByQuests.Add(npc, new List<QuestData>());
                }

                NPCByQuests[npc].Add(questData);
            }
        }
    }
}