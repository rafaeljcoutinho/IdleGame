using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestInfoView : MonoBehaviour
{
    [SerializeField] private Button openChestButton;
    [SerializeField] private CanvasGroup containerCg;
    [SerializeField] private TextMeshProUGUI chestCountText;
    private Tween fadeAnim;

    void Start()
    {
        var slayerManager = Services.Container.Resolve<InventoryService>().PlayerProfile.SlayerManager;
        openChestButton.onClick.RemoveAllListeners();
        openChestButton.onClick.AddListener(OnButtonAction);
        Reset();
        slayerManager.OnNewChest += Reset;
    }

    private void OnButtonAction()
    {
        var slayerManager = Services.Container.Resolve<InventoryService>().PlayerProfile.SlayerManager;
        var chestCount = slayerManager.Chests.Count;
        var rewards = slayerManager.OpenChests();
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.title = "CHEST REWARDS";
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.subtitle = "x" + chestCount;
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.showVideoButton = false;
        var rewardsItems = OverlayCanvas.Instance.AfkProgressPopup._ViewData.drops = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.ToItemDict(rewards);
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.drops = rewardsItems;
        OverlayCanvas.Instance.AfkProgressPopup._ViewData.leftImage = null;
        Services.Container.Resolve<Wallet>().AddPendingItems(Wallet.RewardSource.KillstreakChest, rewards);
        OverlayCanvas.Instance.AfkProgressPopup.Show(result =>
        {
            var rewards = Services.Container.Resolve<Wallet>().ApplyPendingItems(Wallet.RewardSource.KillstreakChest);
            Services.Container.Resolve<InventoryService>().Save();            
            OverlayCanvas.Instance.ShowDrops(rewards);
        });
        Hide();
    }

    private void Reset()
    {
        var chestCount = Services.Container.Resolve<InventoryService>().PlayerProfile.SlayerManager.Chests.Count;
        if (chestCount > 0)
        {
            chestCountText.text = string.Format("<size=75%>x</size>{0}", chestCount);
            Show();
        }
    }

    public void Show()
    {
        fadeAnim?.Kill();
        fadeAnim = containerCg.DOFade(1f, .1f);
        containerCg.interactable = true;
        containerCg.blocksRaycasts = true;
    }

    public void Hide(bool instant = false)
    {
        fadeAnim?.Kill();
        if(instant)
        {
            containerCg.alpha = 0;
        }else
        {
            fadeAnim = containerCg.DOFade(0f, .1f);
        }
        containerCg.interactable = false;
        containerCg.blocksRaycasts = false;

    }
}
