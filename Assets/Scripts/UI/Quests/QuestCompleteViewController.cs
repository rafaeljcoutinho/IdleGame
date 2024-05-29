using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestCompleteViewController : MonoBehaviour
{
    [SerializeField] private QuestCompleteView questCompleteView;

    private List<ItemWithQuantity> items = new();
    private List<Guid> completedQuests = new();

    public void Show(List<Guid> guids)    
    {
        completedQuests = guids;
        Show(completedQuests[0]);
    }

    private void Show(Guid questId)
    {
        completedQuests.Remove(questId);
        var questDataBase = Services.Container.Resolve<QuestDatabaseService>().QuestDatabase;
        var questData = questDataBase.GetQuest(questId);

        foreach (var reward in questData.Rewards)
        {
            items.Add(new ItemWithQuantity{
                item = reward.item,
                quantity = reward.quantity,
            } );
        }

        var viewData = new CompleteQuestViewData(){
            questName = questData.LocalizationKey,
            questId = questId,
            questIcon = questData.questIcon,
            acceptAction = ()=> CompleteAction(questData),
            rewards = items,
        };
        questCompleteView.Show(viewData);
        OverlayCanvas.Instance.GameplaySceneBootstrapper.Camera.GetComponent<ParticleSystemCam>().Show();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        OverlayCanvas.Instance.GameplaySceneBootstrapper.Camera.GetComponent<ParticleSystemCam>().Hide();
        gameObject.SetActive(false);
        questCompleteView.Hide();
        ClearData();
    }

    private void ClearData()
    {
        items.Clear();
    }

    private void CompleteAction(QuestData questData)
    {
        var questsProgressContainer = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer;

        questsProgressContainer.CompleteQuest(questData);
        if (completedQuests.Count >= 1)
        {
            Show(completedQuests[0]);
        }
        else
        {
            Hide();
        }
    }
}
