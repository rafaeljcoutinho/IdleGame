using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class SkillCondensedView : MonoBehaviour
{
    [SerializeField] private List<StatsQuickView.AttributeInfoView> attributeInfoViews;
    
    private List<TweenText> tweens = new();
    private Dictionary<Attribute, float> atributeValue = new();

    private void OnEnable()
    {
        UpdateCondensedView(null);
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged += UpdateCondensedView;
    }

    private void OnDisable()
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged -= UpdateCondensedView;
        ClearTweens();
    }

    void UpdateCondensedView(PlayerEquipmentManager.EquippedItemsChangedArgs _)
    {
        ClearTweens();
        var skillService = Services.Container.Resolve<SkillService>();
        var equipedItems = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.GetAllEquippedItems();
        var equippedItemOnHand = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.GetItemOnSlot(Equipment.EquipSlot.MainHand) as Equipment;
        var combatStyle = equippedItemOnHand == null ? SkillData.Type.Melee : equippedItemOnHand.MainType;
        var modifiers = skillService.GetCombinedModifiers(equipedItems, null);
        var skills = new List<SkillData.Type> { SkillData.Type.Character, combatStyle };
        foreach (var skill in skills)
        {
            var skillInfo = skillService.GetPlayerSkillInfo(skill);
            var skillModifier = skillService.GetParametersForLevel(skill, skillInfo.Level);
            modifiers.Combine(skillModifier);
        }

        foreach (var attributeInfoView in attributeInfoViews)
        {
            var initialText = attributeInfoView.label.text;

            if (!atributeValue.ContainsKey(attributeInfoView.attribute))
            {
                atributeValue.Add(attributeInfoView.attribute, 0f);
            }
            var isBetter = atributeValue[attributeInfoView.attribute] < modifiers.GetAttributeChange(attributeInfoView.attribute)?.Value;
            atributeValue[attributeInfoView.attribute] = modifiers.GetAttributeChange(attributeInfoView.attribute)?.Value ?? 0f;

            attributeInfoView.label.text = SkillExperienceTable.Format(modifiers.GetAttributeChange(attributeInfoView.attribute)?.Value ?? 0f) + attributeInfoView.attribute.Suffix;

            if (initialText != attributeInfoView.label.text)
            {
                BlinkText(attributeInfoView.label, isBetter);
            }
        }
    }

    private void ClearTweens()
    {
        foreach (var tweenColorText in tweens)
        {
            tweenColorText.sequence?.Kill();
            tweenColorText.textMeshProUGUI.color = tweenColorText.color;
        }
        tweens.Clear();
    }

    private void BlinkText(TextMeshProUGUI textMeshProUGUI, bool isBetter)
    {
        var tween = DOTween.Sequence();
        Color originalColor = textMeshProUGUI.color; 
        var color = isBetter? Color.green : Color.red;

        tween.Join(textMeshProUGUI.DOColor(color, 0.15f))
        .Join(textMeshProUGUI.transform.DOPunchScale(Vector3.one * .1f, 0.3f, 2))
        .Append(textMeshProUGUI.DOColor(originalColor, 0.3f));
        tweens.Add(new TweenText{
            sequence = tween, 
            color = originalColor, 
            textMeshProUGUI = textMeshProUGUI
        });
    }

    private class TweenText
    {
        public Sequence sequence;
        public Color color;
        public TextMeshProUGUI textMeshProUGUI;
    }

}

