using System;
using System.Collections.Generic;
using System.Linq;
using Slayer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlayersLodgeUiView : MonoBehaviour
{
    [SerializeField] private ClaimableRewardListItem rewardListItemPrefab;
    [SerializeField] private Transform rewardListParentTransform;
    [SerializeField] private ItemIconPrefab itemIconPrefab;
    [SerializeField] private Transform possibleDropsParentTransform;
    [SerializeField] private Image slayersLodgeImage;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private GameObject maxLevelGameObject;
    [SerializeField] private TextMeshProUGUI upgradeCost;
    [SerializeField] private Image upgradeCostImage;
    [SerializeField] private TextMeshProUGUI lodgeLevel;
    [SerializeField] private TextMeshProUGUI totalKills;
    [SerializeField] private Blocker blocker;

    private List<ClaimableRewardListItem> listRewards = new();
    private List<ItemIconPrefab> listPossibleDrops = new();
    private SlayerManager slayerManager;
    private SlayersLodgeConfig slayersLodgeConfig;
    private SlayersLodgeProgression currentSlayersLodgeProgression;
    public Action OnHide;

    const string titleFormat = "Slayers Lodge  <size=75%>Lv.</size>{0}";
    const string killCountFormat = "{0} kills";
    public void Setup(SlayersLodgeNode node)
    {
        slayerManager = Services.Container.Resolve<InventoryService>().PlayerProfile.SlayerManager;
        slayersLodgeConfig = node.slayersLodgeData;

        currentSlayersLodgeProgression = slayerManager.GetOrCreateProgression(slayersLodgeConfig.Uuid);
        slayersLodgeImage.sprite = slayersLodgeConfig.enemy.icon;

        lodgeLevel.text = string.Format(titleFormat, currentSlayersLodgeProgression.level);
        totalKills.text = string.Format(killCountFormat, SkillExperienceTable.Format(Services.Container.Resolve<InventoryService>().PlayerProfile.StatsCollector.KillCountStats.GetKC(slayersLodgeConfig.enemy.Uuid)));
        PopulateUpgradeList();
        SetAllPossibleDropsItemsView();

        RefreshUpdateButton();
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
    }

    private const string TOTAL_WALLET_AND_COST = "{0}/{1}";

    private void RefreshUpdateButton()
    {
        if (!slayerManager.IsMaxLevel(slayersLodgeConfig.Uuid))
        {
            var itemCost = slayersLodgeConfig.UpgradeConfigs[currentSlayersLodgeProgression.level].Costs[0];
            upgradeCost.text = String.Format(TOTAL_WALLET_AND_COST, 
                                            Services.Container.Resolve<Wallet>().ItemAmount(itemCost.item.Uuid),
                                            SkillExperienceTable.Format(itemCost.quantity));
            upgradeCostImage.sprite = slayersLodgeConfig.UpgradeConfigs[currentSlayersLodgeProgression.level].Costs[0]
                .item.SmallThumbnail;
        }
        else
        {
            upgradeCost.text = "";
            upgradeButton.gameObject.SetActive(false);
            maxLevelGameObject.SetActive(true);
            return;
        }

        var wallet = Services.Container.Resolve<Wallet>();
        var upgradeConfig = slayersLodgeConfig.UpgradeConfigs[currentSlayersLodgeProgression.level];
        if (wallet.CanBuy(upgradeConfig.Costs.ToIdAmountDict()))
        {
            //enable upgrade button color
            upgradeButton.interactable = true;
        }
        else
        {
            //disable upgrade button color
            upgradeButton.interactable = false;
        }
    }

    private void SetAllPossibleDropsItemsView()
    {
        //hardcoded select the unique droptable
        var droptable = slayersLodgeConfig.enemy.droptable;
        var possibleDrops = new List<Droptable.Drop>();
        possibleDrops.AddRange(droptable.RandomDrops);
        possibleDrops.AddRange(droptable.GuaranteedDrops);
        foreach (var droptableUnlock in slayerManager.GetAllUnlocksForDroptable(droptable))
        {
            possibleDrops.Add(droptableUnlock.ToDroptableDrop());
        }

        possibleDrops = possibleDrops.OrderBy(t => t.item.rarity).ToList();

        PopulateItemDrops(possibleDrops);
    }

    private void PopulateItemDrops(List<Droptable.Drop> possibleDrops)
    {
        var hlg = possibleDropsParentTransform.GetComponent<HorizontalLayoutGroup>();
        var padding = hlg.padding.top + hlg.padding.bottom;
        var height = (hlg.transform as RectTransform).rect.height - padding;
        foreach (var possibleDrop in possibleDrops)
        {
            var go = Instantiate(itemIconPrefab, possibleDropsParentTransform);
            listPossibleDrops.Add(go);
            go.SetData(new InventoryItemViewData
            {
                itemData = possibleDrop.item,
            });
            if (possibleDrop.minAmount == possibleDrop.maxAmount)
            {
                go.SetQuantity(SkillExperienceTable.Format(possibleDrop.minAmount));
            }
            else
            {
                go.SetQuantity(
                    $"{SkillExperienceTable.Format(possibleDrop.minAmount)} - {SkillExperienceTable.Format(possibleDrop.maxAmount)}");
            }

            (go.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }

    private void OnUpgradeButtonClick()
    {
        var result = slayerManager.Upgrade(slayersLodgeConfig.Uuid);
        if (result)
        {
            Debug.Log("upgradeSuccess");
            lodgeLevel.text = "Rewards Lv." + currentSlayersLodgeProgression.level.ToString();
            RefreshUpdateButton();
            listRewards[currentSlayersLodgeProgression.level-1]?.EnableToClaim();
            listRewards[currentSlayersLodgeProgression.level-1]?.SetClaimButtonText(false, false);
        }
        else
        {
            Debug.LogError("upgradeFail");
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        blocker.Show(Hide);
    }

    public void Hide()
    {
        blocker.Hide();
        CleanData();
        gameObject.SetActive(false);
        OnHide?.Invoke();
    }

    private void CleanData()
    {
        CleanRewardsList();
        CleanPossibleDropsList();
    }

    private void CleanRewardsList()
    {
        foreach (var go in listRewards)
        {
            if(go == null) continue;
            Destroy(go.gameObject);
        }

        listRewards.Clear();
    }

    private void CleanPossibleDropsList()
    {
        foreach (var go in listPossibleDrops)
        {
            Destroy(go.gameObject);
        }

        listPossibleDrops.Clear();
    }

    private const string ITEM_REWARD = "Lv. {0} - Item reward";
    private const string DROP_UNLOCK = "Lv. {0} - Unlock new drop";
    private const string LOCAL_BUFF = "Lv. {0} - Buff against {1}";
    private const string GLOBAL_BUFF = "Lv. {0} - Global buff";
    
    private void PopulateUpgradeList()
    {
        for (int i = 0; i < slayersLodgeConfig.UpgradeConfigs.Count; i++)
        {
            
            if(!slayersLodgeConfig.UpgradeConfigs[i].showUI){
                listRewards.Add(null);
                continue;
            }
            
            var go = Instantiate(rewardListItemPrefab, rewardListParentTransform);
            listRewards.Add(go);
            var isLocked = currentSlayersLodgeProgression.level <= i;
            if (!isLocked)
            {
                go.EnableToClaim();
            }

            
            var title = "Lv." + (i + 1);
            var description = "DESCRIPTION FOR MODIFIERS IS BROKEN";
            if (description != "")
            {
                if (slayersLodgeConfig.UpgradeConfigs[i].modifiers.global)
                {
                    title = string.Format(GLOBAL_BUFF, i + 1);
                }
                else
                {
                    title = string.Format(LOCAL_BUFF, i + 1, slayersLodgeConfig.enemy.name);
                }
            }

            foreach (var rewards in slayersLodgeConfig.UpgradeConfigs[i].Rewards)
            {
                go.SetItemIconPrefab(new InventoryItemViewData
                {
                    itemData = rewards.item,
                    itemQuantity = rewards.quantity,
                });
                title = string.Format(ITEM_REWARD, i + 1);
            }

            if (slayersLodgeConfig.UpgradeConfigs[i].dropTableUnlock.droptable != null)
            {
                var item = slayersLodgeConfig.UpgradeConfigs[i].dropTableUnlock.item;
                var quantity = 1;
                go.SetItemIconPrefab(new InventoryItemViewData
                {
                    itemData = item,
                    itemQuantity = quantity,
                });
                title = string.Format(DROP_UNLOCK, i + 1);
            }

            var j = i;
            go.SetupText(title, description);
            var isClaimed = currentSlayersLodgeProgression.IsUpgradeUnlocked(j);
            go.SetupButton(() => OnClaimButtonClick(j));
            go.SetClaimButtonText(isClaimed, isLocked);
        }
    }

    private void OnClaimButtonClick(int index)
    {
        var claimed = slayerManager.Claim(slayersLodgeConfig.Uuid, index);
        if (claimed)
        {
            listRewards[index].SetClaimButtonText(true, false);
            if (listRewards[index].ItemIconPrefab.activeInHierarchy)
            {
//                CleanPossibleDropsList();
//                SetAllPossibleDropsItemsView();
            }
        }
        else
        {
            Debug.LogError("claim fail");
        }
    }
}