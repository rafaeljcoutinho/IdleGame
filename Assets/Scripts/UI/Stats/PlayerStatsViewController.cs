using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsViewController : MonoBehaviour
{
    [SerializeField] private Image playerHealthFillAmount;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private Image PlayerCharacterLevelAmount;
    [SerializeField] private TextMeshProUGUI playerCharacterLevelText;
    private GameplaySceneBootstrapper gameplaySceneInstance => OverlayCanvas.Instance.GameplaySceneBootstrapper;

    public void OnSceneChange()
    {
        UpdatePlayerHealthUI();
        UpdateCharacterLevelUI();
        gameplaySceneInstance.Player.HpController.OnTakeDamage += OnOnPlayerTakeDamage;
        gameplaySceneInstance.Player.HpController.OnHeal += OnOnPlayerHeal;

    }

    private void Start() {
        Services.Container.Resolve<SkillService>().OnXpIncrease += OnOnCharacterXpIncrease;
    }

    private void OnOnCharacterXpIncrease(SkillData.Type skill)
    {
        if (skill == SkillData.Type.Character)
            UpdateCharacterLevelUI();
    }

    private const string LEVEL = "<size=75%>Lv.</size>{0}";
    private void UpdateCharacterLevelUI()
    {
        var characterSkillInfo = Services.Container.Resolve<SkillService>().GetPlayerSkillInfo(SkillData.Type.Character);
        float amount = (float)characterSkillInfo.XpInLevelNormalized;
        PlayerCharacterLevelAmount.fillAmount = amount;
        if (playerCharacterLevelText.text != characterSkillInfo.Level.ToString())
        {
            playerCharacterLevelText.text = string.Format(LEVEL, characterSkillInfo.Level);
            UpdatePlayerHealthUI();
        }
    }

    private void OnOnPlayerTakeDamage(HitInfo info)
    {
        UpdatePlayerHealthUI();
    }

    private void OnOnPlayerHeal(int amount)
    {
        UpdatePlayerHealthUI();
    }

    private void UpdatePlayerHealthUI()
    {
        var playerHpController = gameplaySceneInstance.Player.HpController;

        float fillAmount = playerHpController.NormalizedHp;
        if (playerHpController.CurrentHp < 0)
        {
            playerHealthText.text = "0";
            fillAmount = 0;
        }
        else
        {
            playerHealthText.text = playerHpController.CurrentHp.ToString();
        }

        playerHealthFillAmount.fillAmount = fillAmount;
    }
}