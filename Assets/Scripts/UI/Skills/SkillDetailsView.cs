using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetailsView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillLevel;
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI totalXpEarned;
    [SerializeField] private TextMeshProUGUI xpLeftToNextLevel;
    [SerializeField] private TextMeshProUGUI currentXpToNextLevel;
    [SerializeField] private TextMeshProUGUI totalXpToNextLevel;
    [SerializeField] private Slider xpSlider;
    private bool isLocked;
    private SkillData.Type currentSkill;

    private const string LEVEL_LABEL_TEXT = "<size=75%>Lv.</size> {0}";

    public void SetViewData(SkillData.Type skill)
    {
        currentSkill = skill;
        
        var skillService = Services.Container.Resolve<SkillService>();
        var levelInfo = skillService.GetPlayerSkillInfo(skill);
        var skillData = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(skill);

        skillIcon.sprite = skillData.SkillIcon;
        skillName.text = skillData.name.ToUpper();
        skillLevel.text = string.Format(LEVEL_LABEL_TEXT, levelInfo.Level.ToString());
        totalXpEarned.text = SkillExperienceTable.Format(levelInfo.TotalXp);
        xpLeftToNextLevel.text = SkillExperienceTable.Format(levelInfo.TotalXpToNextLevel - levelInfo.XpInLevel);
        currentXpToNextLevel.text = SkillExperienceTable.Format(levelInfo.XpInLevel);
        totalXpToNextLevel.text = SkillExperienceTable.Format(levelInfo.TotalXpToNextLevel);
        xpSlider.value = (float)levelInfo.XpInLevelNormalized;
    }

    public void RefreshUI()
    {
        SetViewData(currentSkill);
    }
}
