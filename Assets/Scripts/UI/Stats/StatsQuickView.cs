using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatsQuickView : MonoBehaviour, IDragHandler
{
    [Serializable]
    public class ColorForDamage
    {
        public Color color;
        public long minDamage;
    }

    [Serializable]
    public class AttributeInfoView
    {
        public TextMeshProUGUI label;
        public Attribute attribute;
    }

    [SerializeField] private Canvas canvas;
    [SerializeField] private StatsView statsView;
    [SerializeField] private TextMeshProUGUI totalDamageValue;
    [SerializeField] private TextMeshProUGUI dpsValue;
    [SerializeField] private List<Attribute> attributesToShowForDamage;
    [SerializeField] private List<Attribute> attributesToShowForSkills;
    [SerializeField] private List<ColorForDamage> orderedColorForDamage;

    [SerializeField] private List<AttributeInfoView> attributeInfoViews;

    public bool IsShowing { private set; get; }
    public bool IsShowingDamage { private set; get; }

    private Color GetColorForDamage(long damage)
    {
        for (var i = orderedColorForDamage.Count - 1; i >= 0; i--)
        {
            if (damage > orderedColorForDamage[i].minDamage)
            {
                return orderedColorForDamage[i].color;
            }
        }

        return orderedColorForDamage[0].color;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        (transform as RectTransform).anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    private void OnEnable()
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged += Redraw;
    }

    private void OnDisable()
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged -= Redraw;
    }

    void Redraw(PlayerEquipmentManager.EquippedItemsChangedArgs _)
    {
        if (IsShowingDamage)
        {
            ShowDamage();
        }
        else
        {
            ShowSkills();
        }
    }

    public void ShowDamage()
    {
        IsShowingDamage = true;
        IsShowing = true;
        var equippedItemOnHand = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.GetItemOnSlot(Equipment.EquipSlot.MainHand) as Equipment;
        var combatStyle = equippedItemOnHand == null ? SkillData.Type.Melee : equippedItemOnHand.MainType;
        var attributesToShow = new List<Attribute>();
        attributesToShow.AddRange(attributesToShowForDamage);

        var combatStyleData = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(combatStyle);
        attributesToShow.Add(combatStyleData.power);
        attributesToShow.Add(combatStyleData.powerMulti);
        attributesToShow.Add(combatStyleData.bonusXp);
        
        statsView.ShowStats(attributesToShow, new List<SkillData.Type> { SkillData.Type.Character, combatStyle});
        gameObject.SetActive(true);

        var progressParameters = Services.Container.Resolve<SkillService>().GetCombinedProgressParameters(combatStyle, null);
        var damage = progressParameters.BasePower;
        var dps = damage * progressParameters.ActionFreqHz;

        totalDamageValue.transform.parent.gameObject.SetActive(true); // dont do this rafa, i can do it tho
        dpsValue.transform.parent.gameObject.SetActive(true); // dont do this rafa, i can do it tho
        totalDamageValue.color = GetColorForDamage(damage);
        dpsValue.color = GetColorForDamage(damage);
        totalDamageValue.text = SkillExperienceTable.Format(damage);
        dpsValue.text = SkillExperienceTable.Format(dps) + "<size=75%>/s</size>";
        IsShowing = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate((transform as RectTransform));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        IsShowing = false;
    }
    
    public void ShowSkills()
    {
        IsShowingDamage = false;
        totalDamageValue.transform.parent.gameObject.SetActive(false); // dont do this rafa, i can do it tho
        dpsValue.transform.parent.gameObject.SetActive(false); // dont do this rafa, i can do it tho
        statsView.ShowStats(attributesToShowForSkills, new List<SkillData.Type> { SkillData.Type.Cooking, SkillData.Type.Crafting, SkillData.Type.Fishing, SkillData.Type.Woodcutting, SkillData.Type.Mining});
        gameObject.SetActive(true);
        IsShowing = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate((transform as RectTransform));
    }
}