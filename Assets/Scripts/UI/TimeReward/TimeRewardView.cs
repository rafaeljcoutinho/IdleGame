using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeRewardView : MonoBehaviour
{
    [SerializeField] private TimeRewardItemList timeRewardItemListPrefab; 
    [SerializeField] private TimeRewardListData timeRewardData;
    [SerializeField] private GameObject rewardDetailsCanvas;
    [SerializeField] private List<TimeRewardItemPrefab> itemIconPrefabs;
    [SerializeField] private TextMeshProUGUI timeDetailsText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button closeButton;


    private long finishTime ;
    private int currentRewardIndex;
    private bool loaded = false;
    private bool canClain = false;
    private Dictionary<Guid, long> rewards = new();

    public void Init()
    {
        var rewardManager = Services.Container.Resolve<InventoryService>().PlayerProfile.timeRewardManager;
        currentRewardIndex = rewardManager.currentRewardIndex;
        LoadReward(currentRewardIndex);
    }

    private void Update() {
        if (!loaded || canClain) return;

        if( DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds >= finishTime - 1)
        {
            CanClain();
            return;
        }

        var remainingTimeInSeconds = finishTime - DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

        TimeSpan remainingTime = TimeSpan.FromSeconds(remainingTimeInSeconds);

        int days = remainingTime.Days;
        int hours = remainingTime.Hours + days * 24;
        int minutes = remainingTime.Minutes;
        int seconds = remainingTime.Seconds;

        string formattedTime = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        if (rewardDetailsCanvas.activeInHierarchy)
        {
            timeDetailsText.text = formattedTime;
        }
        timeRewardItemListPrefab.SetTime(formattedTime);

    }

    private void CanClain()
    {
        timeRewardItemListPrefab.SetTime("");
        claimButton.gameObject.SetActive(true);
        timeDetailsText.gameObject.SetActive(false);
        canClain = true;
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(()=> ClaimReward(currentRewardIndex));
        Services.Container.Resolve<InventoryService>().PlayerProfile.timeRewardManager.canCollect = true;
        timeRewardItemListPrefab.EnableReward(true);
    }

    private void LoadReward(int index)
    {
        if(index >= timeRewardData.timeRewardDatas.Count) return;

        var itemListData = timeRewardData.timeRewardDatas[index];

        canClain = Services.Container.Resolve<InventoryService>().PlayerProfile.timeRewardManager.canCollect;
        if (canClain)
        {
            CanClain();
        }


        if (!Services.Container.Resolve<InventoryService>().PlayerProfile.timeRewardManager.loaded)
        {
            var days = timeRewardData.timeRewardDatas[currentRewardIndex].Days;
            var hours = timeRewardData.timeRewardDatas[currentRewardIndex].Hours;
            var minutes = timeRewardData.timeRewardDatas[currentRewardIndex].Minutes;
            finishTime = (long)DateTime.UtcNow.AddDays(days).AddHours(hours).AddMinutes(minutes).Subtract(DateTime.UnixEpoch).TotalSeconds;
            Services.Container.Resolve<InventoryService>().PlayerProfile.timeRewardManager.finishTime = finishTime;
            Services.Container.Resolve<InventoryService>().PlayerProfile.timeRewardManager.loaded = true;
        }
        finishTime = Services.Container.Resolve<InventoryService>().PlayerProfile.timeRewardManager.finishTime;


        int i = index;
        timeRewardItemListPrefab.EnableReward(canClain);
        timeRewardItemListPrefab.SetData(itemListData.Icon, ()=> ClickInfo(i));
        loaded = true;
        Update();
        gameObject.SetActive(true);
    }

    private void ClaimReward(int index)
    {
        if (!canClain) return;

        rewards.Clear();
        var rewardData = timeRewardData.timeRewardDatas[index];

        foreach (var reward in rewardData.Rewards)
        {
            rewards.Add(reward.item.Uuid, reward.quantity);
        }
        Services.Container.Resolve<InventoryService>().PlayerProfile.timeRewardManager.ClaimItems(rewards);
        gameObject.SetActive(false);
        HideDetails();
        claimButton.gameObject.SetActive(false);
        timeDetailsText.gameObject.SetActive(true);

        Init();
    }

    private void ClickInfo(int index)
    {
        var data = timeRewardData;
        for (int i = 0; i < 3; i++)
        {
            if (i >= data.timeRewardDatas[index].Rewards.Count)
            {
                itemIconPrefabs[i].gameObject.SetActive(false);
                continue;
            }

            itemIconPrefabs[i].SetData(data.timeRewardDatas[index].Rewards[i]);
            itemIconPrefabs[i].SetButton(OnItemClick);
            itemIconPrefabs[i].gameObject.SetActive(true);
        }
        closeButton.gameObject.SetActive(true);
        rewardDetailsCanvas.SetActive(true);
    }

    public void HideDetails()
    {
        rewardDetailsCanvas.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    public void OnItemClick(ItemWithQuantity itemWithQuantity, Vector3 position)
    {
        OverlayCanvas.Instance.ToolTip.Show(itemWithQuantity.item, position, ConsumableItemResolver.Source.Inventory, true);
    }


}
