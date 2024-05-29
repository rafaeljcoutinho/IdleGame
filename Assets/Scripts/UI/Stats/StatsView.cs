using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsView : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> statsNames;
    [SerializeField] private List<TextMeshProUGUI> statsValues;
    
    public void ShowStats(List<Attribute> attributesDefinitions, List<SkillData.Type> skills)
    {
        var skillService = Services.Container.Resolve<SkillService>();
        var equipedItems = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.GetAllEquippedItems();
        var modifiers = skillService.GetCombinedModifiers(equipedItems, null);

        foreach (var skill in skills)
        {
            var skillInfo = skillService.GetPlayerSkillInfo(skill);
            var skillModifier = skillService.GetParametersForLevel(skill, skillInfo.Level);
            modifiers.Combine(skillModifier);
        }

        for (var i = 0; i < attributesDefinitions.Count; i++)
        {
            statsNames[i].text = attributesDefinitions[i].LocalizedName;
            var attributeChange = modifiers.GetAttributeChange(attributesDefinitions[i]);
            if (attributeChange != null)
            {
                statsValues[i].text = SkillExperienceTable.Format(attributeChange.Value) + attributeChange.attribute.Suffix;   
            }
            else
            {
                statsValues[i].text = 0 + attributesDefinitions[i].Suffix;   
            }
            statsNames[i].gameObject.SetActive(true);
            statsValues[i].gameObject.SetActive(true);
        }

        for (var i = attributesDefinitions.Count; i < statsNames.Count; i++)
        {
            statsNames[i].gameObject.SetActive(false);
            statsValues[i].gameObject.SetActive(false);
        }
    }

}
