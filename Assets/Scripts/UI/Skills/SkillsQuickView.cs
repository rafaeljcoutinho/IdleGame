using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillsQuickView : MonoBehaviour, IDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private StatsView statsView;
    [SerializeField] private TextMeshProUGUI totalExperienceLabel;
    [SerializeField] private TextMeshProUGUI nextLevelExperienceLabel;
    [Header("Slider")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI targetExpLabel;
    [SerializeField] private TextMeshProUGUI currentExpInLevelLabel;
    [SerializeField] private TextMeshProUGUI skillName;

    [Header("Collapse")] 
    [SerializeField] private TextMeshProUGUI collapseLabel;
    [SerializeField] private List<GameObject> disableOnCollapse;

    private SkillData skillData;
    private bool isCollapsed;
    private const string SkillNameFormat = "{0}  <size=50%>Lv.</size><size=75%>{1}";
    
    public bool IsShowing { private set; get; }
    public bool IsShowingDamage { private set; get; }
    
    public void OnDrag(PointerEventData eventData)
    {
        (transform as RectTransform).anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    private void OnEnable()
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged += Redraw;
        Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.OnItemAmountChanged += Redraw;
    }

    private void OnDisable()
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged -= Redraw;
        Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.OnItemAmountChanged -= Redraw;
    }

    public void Collapse()
    {
        isCollapsed = !isCollapsed;
        foreach (var go in disableOnCollapse)
        {
            go.SetActive(!isCollapsed);
        }

        var rotation = new Vector3(0, 0, isCollapsed ? 180 : 0);
        collapseLabel.transform.rotation = Quaternion.Euler(rotation);
        LayoutRebuilder.ForceRebuildLayoutImmediate((transform as RectTransform));
    }

    void Redraw(Dictionary<Guid, long> _)
    {
        UpdateData();
    }
    
    void Redraw(PlayerEquipmentManager.EquippedItemsChangedArgs _)
    {
        UpdateData();
    }

    public void SetSkill(SkillData.Type skill)
    {
        skillData = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(skill);
    }
    
    public void UpdateData()
    {
        var skillInfo = Services.Container.Resolve<SkillService>().GetPlayerSkillInfo(skillData.SkillType);
        skillName.text = string.Format(SkillNameFormat, skillData.NameKey, skillInfo.Level);
        totalExperienceLabel.text = SkillExperienceTable.Format(skillInfo.TotalXp);
        nextLevelExperienceLabel.text = SkillExperienceTable.Format(skillInfo.TotalXpToNextLevel-skillInfo.XpInLevel);
        targetExpLabel.text = SkillExperienceTable.Format(skillInfo.TotalXpToNextLevel);
        currentExpInLevelLabel.text = SkillExperienceTable.Format(skillInfo.XpInLevel);
        expSlider.value = (float) skillInfo.XpInLevelNormalized;

        var attributesToShow = new List<Attribute>();
    
        if (skillData.power) {
            attributesToShow.Add(skillData.power);
        }
        if (skillData.powerMulti) {
            attributesToShow.Add(skillData.powerMulti);
        }
        if (skillData.speedMulti) {
            attributesToShow.Add(skillData.speedMulti);
        }
        if (skillData.critChance) {
            attributesToShow.Add(skillData.critChance);
        }
        if (skillData.critDamage) {
            attributesToShow.Add(skillData.critDamage);
        }
        if (skillData.range) {
            attributesToShow.Add(skillData.range);
        }
        if (skillData.accu) {
            attributesToShow.Add(skillData.accu);
        }
        if (skillData.bonusXp) {
            attributesToShow.Add(skillData.bonusXp);
        }

        statsView.ShowStats(attributesToShow, new List<SkillData.Type> { SkillData.Type.Character, skillData.SkillType});
        Debug.Log("Updated view");
        LayoutRebuilder.ForceRebuildLayoutImmediate((transform as RectTransform));
    }

    public void Show()
    {
        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate((transform as RectTransform));
        IsShowing = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        IsShowing = false;
    }
}