using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillLevelInfoView : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI currentLevel;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private SkillData.Type skill;
    [SerializeField] private TextMeshProUGUI expInfoLabel;

    public SkillData.Type Skill => skill;

    public ViewData viewData = new();

    public struct ViewData
    {
        public float experienceNormalized;
        public string currentLevel;
        public string skillName;
        public string currentTargetExp;
    }

    public void Redraw()
    {
        slider.value = viewData.experienceNormalized;
        currentLevel.text = viewData.currentLevel;
        skillName.text = viewData.skillName;
        expInfoLabel.text = viewData.currentTargetExp;
    }
}