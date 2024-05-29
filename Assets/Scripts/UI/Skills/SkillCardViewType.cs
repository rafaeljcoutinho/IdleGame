using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SkillCardViewType : MonoBehaviour
{
    [FormerlySerializedAs("SkillType")] [SerializeField]
    private SkillData.Type skillType;

    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private GameObject selected;

    public SkillData.Type SkillType => skillType;

    public void SetButtonAction(Action<SkillData.Type> action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { action(SkillType); });
    }

    private const string levelText = "{0}";

    public void RefreshUI(bool isSelected)
    {
        selected.SetActive(isSelected);
        icon.sprite = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(skillType).SkillIcon;
        level.text = string.Format(levelText,
            Services.Container.Resolve<SkillService>().GetPlayerSkillInfo(SkillType).Level);
    }
}