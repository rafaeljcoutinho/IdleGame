using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfoView : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillLevel;
    [SerializeField] private TextMeshProUGUI kphLabel;
    [SerializeField] private TextMeshProUGUI xpphLabel;

    [Header("progress bar")] [SerializeField]
    private Slider slider;

    [SerializeField] private TextMeshProUGUI xpCounter;

    public class ViewModel
    {
        public string skillName;
        public string skillLevel;
        public string kphLabel;
        public string xpphLabel;
        public float xpNormalized;
        public string xpCounter;
        public Sprite skillIcon;

        public ViewModel(){}
        
        public ViewModel(string skillName, string skillLevel, string kphLabel, string xpphLabel, float xpNormalized, string xpCounter, Sprite skillIcon)
        {
            this.skillName = skillName;
            this.skillLevel = skillLevel;
            this.kphLabel = kphLabel;
            this.xpphLabel = xpphLabel;
            this.xpNormalized = xpNormalized;
            this.xpCounter = xpCounter;
            this.skillIcon = skillIcon;
        }
        
        public void Set(string skillName, string skillLevel, string kphLabel, string xpphLabel, float xpNormalized, string xpCounter, Sprite skillIcon)
        {
            this.skillName = skillName;
            this.skillLevel = skillLevel;
            this.kphLabel = kphLabel;
            this.xpphLabel = xpphLabel;
            this.xpNormalized = xpNormalized;
            this.xpCounter = xpCounter;
            this.skillIcon = skillIcon;
        }

    }

    private ViewModel viewModel;
    public ViewModel ViewData => viewModel ??= new ViewModel();

    public void RefreshViewData()
    {
        skillName.text = ViewData.skillName;
        skillLevel.text = ViewData.skillLevel;
        kphLabel.text = ViewData.kphLabel;
        xpphLabel.text = ViewData.xpphLabel;
        slider.value = ViewData.xpNormalized; 
        xpCounter.text = ViewData.xpCounter;
        skillIcon.sprite = ViewData.skillIcon;
    }

    public static class ViewModelBuilder
    {
        public static void UpdateViewModel(ViewModel viewModel, SkillData.Type skill)
        {
            var skillService = Services.Container.Resolve<SkillService>();
            var levelInfo = skillService.GetPlayerSkillInfo(skill);
            var skillData = skillService.SkillDatabase.GetSkillData(skill);

            viewModel.kphLabel = "<size=250%>-</size> k/h";
            viewModel.xpphLabel = "<size=250%>-</size> xp/h";
            viewModel.skillName = skillData.NameKey;
            viewModel.xpNormalized = (float)levelInfo.XpInLevelNormalized;
            viewModel.skillIcon = skillData.SkillIcon;
            viewModel.xpCounter = $"{levelInfo.XpInLevel}/{levelInfo.TotalXpToNextLevel}";
            viewModel.skillLevel = $"Lv. {levelInfo.Level}";
        }
    }
}
