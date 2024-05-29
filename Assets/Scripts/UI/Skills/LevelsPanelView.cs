using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelsPanelView : MonoBehaviour
{
    [SerializeField] private RectTransform viewRoot;
    [SerializeField] private List<SkillCardViewType> skillCardViewTypes;
    [SerializeField] private SkillsQuickView skillsQuickView;

    private Wallet Wallet => Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet;
    private SkillData.Type selectedType;

    public void Show()
    {
        selectedType = OverlayCanvas.Instance.GameplaySceneBootstrapper.Player.CurrentSkillBeingUsed();
        if (selectedType == SkillData.Type.None)
        {
            selectedType = skillCardViewTypes[0].SkillType;
        }
        foreach (var skillCard in skillCardViewTypes)
        {
            skillCard.SetButtonAction(OnSkillNameClick);
        }
        viewRoot.gameObject.SetActive(true);
        RefreshUI();
//        skillsQuickView.Show();
//        skillsQuickView.UpdateData();
        Wallet.OnItemAmountChanged += WalletChanged;
    }

    public void OnSkillNameClick(SkillData.Type type)
    {
        selectedType = type;
        RefreshUI();
    }

    public void Hide()
    {
        viewRoot.gameObject.SetActive(false);
        Wallet.OnItemAmountChanged -= WalletChanged;
    }

    private void WalletChanged(Dictionary<Guid, long> _)
    {
        RefreshUI();
    }
    
    
    private void RefreshUI()
    {
        //skillsQuickView.SetSkill(selectedType);
        //skillsQuickView.UpdateData();
        foreach(var card in skillCardViewTypes)
        {
            card.RefreshUI(false);
        }
    }
}