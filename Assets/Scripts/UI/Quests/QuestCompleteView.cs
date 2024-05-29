
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestCompleteView : MonoBehaviour
{
    [SerializeField] private TextItemList rewardPrefab;
    [SerializeField] private Transform rewardsParent;
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Image questIcon;

    private List<GameObject> allPrefabs = new();
    private Guid questId;

    private const string COMPLETE_QUEST_NAME = "{0}";
    private const string REWARDS_TEXT = "{0} <color={1}><u>{2}</u></color>";
    private const string GET_QUEST_TITLE_LOC = "quests.{0}.title";


    public void Show(CompleteQuestViewData viewData)
    {
        questId = viewData.questId;
        var questNameFormated = Services.Container.Resolve<LocalizationService>().LocalizeText(viewData.questName);

        questName.text = string.Format(COMPLETE_QUEST_NAME, questNameFormated);
        questIcon.sprite = viewData.questIcon;
        acceptButton.onClick.AddListener(()=> viewData.acceptAction());

        foreach (var reward in viewData.rewards)
        {
            var go = Instantiate(rewardPrefab, rewardsParent);
            allPrefabs.Add(go.gameObject);

            var item = reward.item;
            var pos = go.transform.position;
            var rarityItemColor = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.RarityColorHexa[item.rarity];

            var quantityFormated = SkillExperienceTable.Format(reward.quantity);

            go.SetText(string.Format(REWARDS_TEXT, quantityFormated, rarityItemColor, item.LocalizationKey));
            go.DisableCircle();
            go.SetButtonAction( ()=> ShowTooltip(item, pos) );
        }
        StartAnimation();
    }

    private void StartAnimation()
    {
        rewardsParent.DOScale(0.1f, 0);
        rewardsParent.gameObject.SetActive(true);
        rewardsParent.DOScale(1f, 0.2f).OnComplete( ()=> 
            rewardsParent.DOScale(1.1f, 0.1f).OnComplete( ()=> 
                rewardsParent.DOScale(1f, 0.1f) ) );
    }

    public void Hide()
    {
        rewardsParent.gameObject.SetActive(false);
        ClearData();
    }

    private void ClearData()
    {
        acceptButton.onClick.RemoveAllListeners();
        questId = Guid.Empty;
        foreach (var go in allPrefabs)
        {
            Destroy(go);
        }
        allPrefabs.Clear();
    }

    private void ShowTooltip(Item item, Vector3 pos)
    {
        OverlayCanvas.Instance.ToolTip.Show(item, pos, ConsumableItemResolver.Source.Inventory, true);
    }
}

public class CompleteQuestViewData
{
    public Guid questId;
    public Sprite questIcon;
    public string questName;
    public List<ItemWithQuantity> rewards;
    public Action acceptAction;
}
