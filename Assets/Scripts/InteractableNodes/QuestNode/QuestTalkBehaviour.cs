using System;
using System.Collections.Generic;
using UnityEngine;
using Woodcutting;

public class QuestTalkBehaviour : Player.Behaviour
{
    private PlayerProfile PlayerProfile => Services.Container.Resolve<InventoryService>().PlayerProfile;
    private QuestProgressContainer QuestProgressContainer => Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
    private QuestNode questNode;
    private const float minDistance = .6f;
    private Player player;
    public override SkillData.Type SkillUsed => SkillData.Type.None;
    public override bool AllowFSMReset => false;
    private bool showingDialog;
    private QuestData questData;

    public QuestTalkBehaviour(Player player)
    {
        this.player = player;
        showingDialog = false;
    }
    
    public override Type Update(float dt)
    {
        if (questData == null)
        {
            player.ResetFSM();
            return typeof(DoNothingBehaviour);
        }
        if (Vector3.Distance(questNode.transform.position, player.transform.position) > minDistance)
        {
            var closeDistance = FSM.GetState<CloseDistanceBehaviour>();
            closeDistance.SetTarget(questNode.transform, minDistance, GetType());
            return typeof(CloseDistanceBehaviour);
        }
        return GetType();
    }


    private List<Guid> completedQuestsIdList = new();

    void ShowDialog()
    {
        showingDialog = true;
        var questProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;
        var progress = questProgressContainer.GetQuestProgress(questData);
        if (!progress.Started)
        {
            OverlayCanvas.Instance.QuestView.Show();
            OverlayCanvas.Instance.QuestView.OpenQuestDetails(questData.Uuid, () =>
            {
                OverlayCanvas.Instance.QuestView.Hide();
                ShowDialog();
            });
            return;
        }
        if (progress.Completed)
        {
            return;
        }

        var currentStep = progress.CurrentInprogressStep();
        var currentDialog = progress.StepProgress[currentStep].currentDialog;

        if (currentDialog >= questData.Steps[currentStep].Dialogs.Count)
        {
            OverlayCanvas.Instance.QuestDialogView.
            ShowObjective(questData.Steps[currentStep], affirmative =>
            {
                if (affirmative)
                {
                    questProgressContainer.CompleteStep(questData, currentStep);  
                    if (progress != null && questProgressContainer.CanCompleteQuest(progress))
                    { 
                        completedQuestsIdList.Clear();
                        completedQuestsIdList.Add(Guid.Parse(questData.id));
                        OverlayCanvas.Instance.QuestCompleteViewController.Show(completedQuestsIdList);
                    }
                }
                OverlayCanvas.Instance.QuestDialogView.Close();
                player.ResetFSM();
            });
        }
        else
        {
            var dialog = questData.Steps[currentStep].Dialogs[currentDialog];
        
            OverlayCanvas.Instance.QuestDialogView.ShowDialog(dialog, questData.Steps[currentStep], ok =>
            {
                Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer.CompleteDialog(questData, currentStep, currentDialog);
                ShowDialog();
            });
        }
    }

    public override void OnForceChange()
    {
        OverlayCanvas.Instance.QuestDialogView.Close();
    }

    public void ShowRandomBark()
    {
        var bark = questNode.Npc.barks[UnityEngine.Random.Range(0,questNode.Npc.barks.Count)];
        var chs = Services.Container.Resolve<ContextualHintService>();
        chs.SetDisplayOptions(new ContextualHintService.HintDisplayOptions
            {
                offset = 2f,
                anchorPosition = Contextual2DObject.AnchorPosition.Top,
                fadeBackground = false,
                dismissOnScreenInteraction = false,
                autoHideSeconds = 3f,
            })
            .SetTargetTransform(questNode.transform)
            .Show(new TextLayout.LayoutData {
                LocalizedText = bark
            }
        );
    }

    public override void SetInteractableNode(IInteractableNode node)
    {
        questNode = node as QuestNode;
        showingDialog = false;
        var npcQuests = Services.Container.Resolve<NPCService>().NPCByQuests;
        if (npcQuests.ContainsKey(questNode.Npc.npcNameKey))
        {
            questData = Services.Container.Resolve<NPCService>().NPCByQuests[questNode.Npc.npcNameKey][0];
        }
        else
        {
            questData = null;
            ShowRandomBark();
            return;
        }

        var questProgress = QuestProgressContainer.GetQuestProgress(questData);
        var started = questProgress != null;
        var completed = questProgress != null && questProgress.Completed;
        if (completed || QuestProgressContainer.CanCompleteQuest(questProgress))
        {
            questData = null;
            ShowRandomBark();
            return;
        }
        
        if (started)
        {
            ShowDialog();
        }
        else
        {
            Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer
                .InitQuestUnlocking(questData);
            OverlayCanvas.Instance.QuestView.Show();
            OverlayCanvas.Instance.QuestView.OpenQuestDetails(questData.Uuid, () =>
            {
                OverlayCanvas.Instance.QuestView.Hide();
                ShowDialog();
            });
        }
    }
}