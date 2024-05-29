using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContextualExpBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private TextMeshProUGUI percentExpLabel;
    [SerializeField] private Image skillIcon;
    [SerializeField] private Slider xpSlider;
    
    private SkillData.Type trackedSkill;
    private Player player;

    public void Init()
    {
        player = OverlayCanvas.Instance.GameplaySceneBootstrapper.Player;
        player.OnBehaviourChanged += BehaviourChanged;
        Services.Container.Resolve<SkillService>().OnXpIncrease += OnXpIncrease;
    }

    private void BehaviourChanged()
    {
        trackedSkill = player.CurrentSkillBeingUsed();
        if (trackedSkill == SkillData.Type.None)
        {
            var weapon = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager
                .GetItemOnSlot(Equipment.EquipSlot.MainHand);
            trackedSkill = weapon != null ? ((Equipment)weapon).MainType : SkillData.Type.Melee;
        }
        UpdateUI();
    }

    private void OnXpIncrease(SkillData.Type skill)
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        var skillInfo = Services.Container.Resolve<SkillService>().GetPlayerSkillInfo(trackedSkill);
        var skillData = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(trackedSkill);
        levelLabel.text = skillInfo.Level.ToString();
        percentExpLabel.text = SkillExperienceTable.Format((float)skillInfo.XpInLevelNormalized * 100) + "%";
        skillIcon.sprite = skillData.SkillIcon;
        xpSlider.value = (float)skillInfo.XpInLevelNormalized;
    }
}
