using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardViewPrefab : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI skillLevel;
    [SerializeField] TextMeshProUGUI skillName;
    [SerializeField] Image skillIcon;
    [SerializeField] Slider experienceSlider;

    private SkillData.Type skill;

 

    public void RefreshUI(){
        SetSkillData(skill);
    }

    public void SetSkillData(SkillData.Type skill){
        this.skill = skill;
        var skillService = Services.Container.Resolve<SkillService>();
        var levelInfo = skillService.GetPlayerSkillInfo(skill);
        var skillData = skillService.SkillDatabase.GetSkillData(skill);

        skillName.text = skillData.NameKey;
        experienceSlider.value = (float)levelInfo.XpInLevelNormalized;
        skillIcon.sprite = skillData.SkillIcon;
        skillLevel.text = $"Lv. {levelInfo.Level}";
    }

}
