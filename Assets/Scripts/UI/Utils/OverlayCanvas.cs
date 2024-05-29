using System;
using System.Collections.Generic;
using Cooking;
using UnityEngine;
using UnityEngine.EventSystems;

public class OverlayCanvas : SingletonBehaviour<OverlayCanvas>
{
    [SerializeField] private ToolTipBehaviour toolTip;
    [SerializeField] private HealthBarManager healthBarManager;
    [SerializeField] private InventoryViewController inventoryViewController;
    [SerializeField] private QuestDialogView questDialogView;
    [SerializeField] private TimeRewardView timeRewardView;
    [SerializeField] private StoreView storeView;
    [SerializeField] private QuestCTAView questCTAView;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private HudViewController hud;
    [SerializeField] private ToastNotificationContainer toastNotificationContainer;
    [SerializeField] private AfkProgressPopup afkProgressPopup;
    [SerializeField] private CookingPopupView cookingPopupView;
    [SerializeField] private DisposableViewPool disposableViewPool;
    [SerializeField] private SlayersLodgeUiView slayersLodgeView;
    [SerializeField] private LevelUpRewardView levelUpRewardView;
    [SerializeField] private ItemDropFeedView itemFeed;
    [SerializeField] private ItemDropCanvasView experienceFeed;
    [SerializeField] private PlayerStatsViewController playerStatsViewController;
    [SerializeField] private AllQuestView questView;
    [SerializeField] private QuestCompleteViewController questCompleteViewController;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private DynamicUIIndicatorContainer dynamicUIIndicatorContainer;
    [SerializeField] private ScreenEffects screenEffects;

    public HealthBarManager HealthBarManager => healthBarManager;
    public InventoryViewController InventoryViewController => inventoryViewController;
    public QuestDialogView QuestDialogView => questDialogView;
    public StoreView StoreView => storeView;
    public HudViewController Hud => hud;
    public TimeRewardView TimeRewardView => timeRewardView;
    public ToastNotificationContainer ToastNotificationContainer => toastNotificationContainer;
    public AfkProgressPopup AfkProgressPopup => afkProgressPopup;
    public CookingPopupView CookingPopupView => cookingPopupView;
    public DisposableViewPool DisposableViewPool => disposableViewPool;
    public SlayersLodgeUiView SlayersLodgeView => slayersLodgeView;
    public GameplaySceneBootstrapper GameplaySceneBootstrapper => gameplaySceneBootstrapper;
    public ToolTipBehaviour ToolTip => toolTip;
    public ItemDropFeedView ItemFeed => itemFeed;
    public QuestCTAView QuestCTAView => questCTAView;
    public AllQuestView QuestView => questView;
    public QuestCompleteViewController QuestCompleteViewController => questCompleteViewController;
    public EventSystem EventSystem => eventSystem;
    public DynamicUIIndicatorContainer DynamicUIIndicatorContainer => dynamicUIIndicatorContainer;
    public ScreenEffects ScreenEffects => screenEffects;

    private Camera cam;
    private GameplaySceneBootstrapper gameplaySceneBootstrapper;

    public void OnSceneChange(Camera camera, GameplaySceneBootstrapper sceneBootstrapper)
    {
        cam = camera;
        gameplaySceneBootstrapper = sceneBootstrapper;
        healthBarManager.Init();
        Hud.Init();
        levelUpRewardView.OnSceneChange();
        playerStatsViewController.OnSceneChange();
        timeRewardView.Init();
    }

    public void ShowDrops(Dictionary<Guid, long> drops, bool showExperience = true, bool showItems = true)
    {
        var items = new Dictionary<Guid, long>();
        var experience = new Dictionary<Guid, long>();
        foreach (var item in drops)
        {
            var itemToShow = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(item.Key);
            if (itemToShow is Experience)
            {
                experience.Add(item.Key, item.Value);
            }
            else if (itemToShow is not FeatureUnlockToken)
            {
                items.Add(item.Key, item.Value);
            }
        }

        if (experience.Count > 0 && showExperience)
        {
            experienceFeed.ShowItems(experience);
        }

        if (items.Count > 0 && showItems)
        {
            ItemFeed.ShowItems(items);
        }
    }
    
    public void AnchorWorldTransform(RectTransform rect, Transform t)
    {
        var pos = cam.WorldToViewportPoint(t.position);

        var portalSize = canvas.rect.size;
        pos.x = (pos.x - 0.5f) * portalSize.x;
        pos.y = (pos.y - 0.5f) * portalSize.y;
        pos.z = 0;

        rect.localPosition = pos;
    }
    
    public void AnchorWorldPos(RectTransform rect, Vector3 position)
    {
        var pos = cam.WorldToViewportPoint(position);

        var portalSize = canvas.rect.size;
        pos.x = (pos.x - 0.5f) * portalSize.x;
        pos.y = (pos.y - 0.5f) * portalSize.y;
        pos.z = 0;

        rect.localPosition = pos;
    }
    
    List<RaycastResult> results = new();

    public bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(eventSystem)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
        results.Clear();
        eventSystem.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}