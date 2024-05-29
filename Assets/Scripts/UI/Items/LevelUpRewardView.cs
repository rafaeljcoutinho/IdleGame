using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpRewardView : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TextMeshProUGUI levelUpLabel;
    [SerializeField] private Image levelUpDivider;
    [SerializeField] private RectTransform statChangesContainer;
    [SerializeField] private StatsChangeView statsChangeViewPrefab;
    [Header("Feature unlock")]
    [SerializeField] private RectTransform featureUnlockContainer;
    [SerializeField] private Image featureIcon;
    [SerializeField] private TextMeshProUGUI featureName;
    [SerializeField] private TextMeshProUGUI featureDescription;

    private Sequence tween;

    private float dismissTimer = 0;
    private bool showing = false;
    private bool loaded = false;
    private List<RewardViewEntry> viewQueue;

    public class RewardViewEntry
    {
        public string Title;
        public SkillData.Type SkillType;
        public string FeatureName;
        public string FeatureDescription;
        public Sprite FeatureIcon;
        public int oldLevel;
        public List<SkillData.PrettyModifierInfo> statChanges;
    }
    
    public void OnSceneChange()
    {
        viewQueue = new List<RewardViewEntry>();

        if (loaded) return;

        Services.Container.Resolve<SkillService>().OnSkillLevelsChanged += OnSkillLevelChanged;
        Services.Container.Resolve<Wallet>().OnItemAmountChanged += OnItemAmountChanged;
        loaded = true;
    }


    private void OnItemAmountChanged(Dictionary<Guid, long> obj)
    {
        foreach (var itemChanges in obj)
        {
            var oldAmt = Services.Container.Resolve<Wallet>().ItemAmount(itemChanges.Key) - itemChanges.Value;
            var isNewItem = oldAmt == 0 && itemChanges.Value > 0; 
            if (isNewItem)
            {
                var itemData = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(itemChanges.Key);
                if (itemData is FeatureUnlockToken)
                {
                    var rewardEntry = new RewardViewEntry
                    {
                        Title = "NEW FEATURE UNLOCK",
                        FeatureIcon = itemData.SmallThumbnail,
                        FeatureName = itemData.LocalizationKey,
                        FeatureDescription = itemData.DescriptionLocalizationKey,
                    };
                    viewQueue.Add(rewardEntry);
                    if (!showing)
                    {
                        ShowNext();
                    }
                }
            }
        }
    }

    private void OnSkillLevelChanged(List<SkillService.LevelupEvent> levelupEvents)
    {
        RewardViewEntry rewardViewEntry = new RewardViewEntry();
        foreach (var levelupEvent in levelupEvents)
        {
            var skill = levelupEvent.skill;
            var oldLevel = levelupEvent.oldLevel;
            var newLevel = levelupEvent.newLevel;

            var skip = false;
            for (int i = 0; i < viewQueue.Count; i++)
            {
                if (viewQueue[i].SkillType == skill)
                {
                    viewQueue[i].statChanges = CreateViewData(skill, viewQueue[i].oldLevel, newLevel);
                    skip = true;
                    break;
                }
            }

            if (skip)
            {
                continue;
            }
            var viewData = CreateViewData(skill, oldLevel, newLevel);
            viewQueue.Add(new RewardViewEntry
            {
                oldLevel = oldLevel,
                SkillType = skill,
                statChanges = viewData,
                Title = "LEVEL UP!",
            });
        }
        
        if (!showing)
        {
            ShowNext();
        }
    }

    List<SkillData.PrettyModifierInfo> CreateViewData(SkillData.Type skill, int oldLevel, int newLevel)
    {
        var skillService = Services.Container.Resolve<SkillService>();
        var oldParameters = skillService.GetParametersForLevel(skill, oldLevel);
        var newParameters = skillService.GetParametersForLevel(skill, newLevel);
        var parameterChanges = SkillData.GetPrettyModifierComparison(newParameters, oldParameters);
        var ans = new List<SkillData.PrettyModifierInfo>
        {
            new ()
            {
                Name = $"{skill.ToString()} lv.",
                DeltaFormatted = $"{oldLevel} -> {newLevel}",
                Delta = newLevel - oldLevel,
            }
        };

        foreach (var paramChange in parameterChanges)
        {
            if (paramChange.Delta > 0)
            {
                ans.Add(paramChange);
            }
        }
        return ans;
    }

    public void ShowNext()
    {
        if (viewQueue.Count == 0)
        {
            return;
        }

        var first = viewQueue[0];
        viewQueue.RemoveAt(0);

        var isLevelup = first.FeatureIcon == null;
        var changes = isLevelup ? first.statChanges : null;
        if (isLevelup) // collapse all level ups to see in a single view
        {
            for (var i = viewQueue.Count-1; i >= 0; i--)
            {
                if (viewQueue[i].FeatureIcon == null)
                {
                    changes.Add(new SkillData.PrettyModifierInfo());
                    changes.AddRange(viewQueue[i].statChanges);
                    viewQueue.RemoveAt(i);
                }
            }
        }

        showing = true;
        cg.alpha = 0;
        levelUpLabel.alpha = 0f;
        levelUpLabel.text = first.Title;

        var col = levelUpDivider.color;
        col.a = 0;
        levelUpDivider.color = col;
        
        tween?.Kill();
        tween = DOTween.Sequence();
        tween
            .Join(cg.DOFade(1, 1f).SetEase(Ease.OutExpo))
            .Join(levelUpLabel.DOFade(1, 1f))
            .Join(levelUpDivider.DOFade(1, 1f));
        
        for (var i = statChangesContainer.transform.childCount-1; i >= 1; i--)
        {
            Destroy(statChangesContainer.transform.GetChild(i).gameObject);
        }

        dismissTimer = Time.time  + 2f;
        if (isLevelup)
        {
            var baseDelay = .3f;
            var delayBetweenItems = .07f;
            var timeToShowAll = baseDelay + changes.Count * delayBetweenItems + delayBetweenItems;
            for (var i = 0; i < changes.Count; i++)
            {
                var instance = Instantiate(statsChangeViewPrefab);
                instance.transform.SetParent(statChangesContainer);
                instance.gameObject.SetActive(true);
                instance.transform.localScale = Vector3.one;
                instance.Show(changes[i], baseDelay + i * delayBetweenItems, timeToShowAll + i * delayBetweenItems);
            }
            dismissTimer += timeToShowAll + .3f * changes.Count;
            featureUnlockContainer.gameObject.SetActive(false);
        }else
        {
            featureUnlockContainer.gameObject.SetActive(true);
            featureIcon.sprite = first.FeatureIcon;
            featureName.text = first.FeatureName;
            featureDescription.text = first.FeatureDescription;
            dismissTimer += 4;
        }
        
        Debug.Log($"Will dismiss after {dismissTimer-Time.time} seconds");
        
        
        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(statChangesContainer);
    }

    private void Update()
    {
        if (Time.time > dismissTimer)
        {
            if (viewQueue.Count == 0 && showing)
            {
                Hide();   
            }
            else
            {
                ShowNext();
            }
        }
    }

    public void Hide()
    {
        showing = false;
        tween?.Kill();
        tween = DOTween.Sequence();
        tween.Join(cg.DOFade(0, .6f).SetEase(Ease.OutExpo));
    }
}
