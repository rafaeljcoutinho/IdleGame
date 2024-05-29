using System;
using System.Collections.Generic;
using DG.Tweening;
using Fishing;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Woodcutting;

[Serializable]
public class HudViewController : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private GameObject inventoryRedDot;
    [SerializeField] private CanvasGroup buttonCG;
    [SerializeField] private TextMeshProUGUI buttonLabel;
    [SerializeField] private Image buttonIcon;
    [SerializeField] private Image inventoryBlink;
    [SerializeField] private Image autoFightButtonBackground;
    [SerializeField] private Button cameraZoomButton;
    [SerializeField] private Joystick joystick;
    [FormerlySerializedAs("killstreakInfoView")] [SerializeField] private ChestInfoView m_chestInfoView;
    [SerializeField] private ContextualExpBar xpBar;

    [SerializeField] private Sprite chopIcon;
    [SerializeField] private Sprite talkIcon;
    [SerializeField] private Sprite interactIcon;
    [SerializeField] private Sprite fightIcon;
    [SerializeField] private Sprite fishIcon;
    
    private bool autoFightEnabled = true;
    public bool AutoFightEnabled => autoFightEnabled;
    public Button CameraZoomButton => cameraZoomButton;
    public ChestInfoView ChestInfoView => m_chestInfoView;
    public Joystick Joystick => joystick;
    
    public Button Button => button;
    public IInteractableNode node;

    private Vector2 initialSizeDelta;
    private Tween tween;
    private bool isInventoryBlinking;

    public void Init()
    {
        initialSizeDelta = Vector2.one;
        xpBar.Init();
    }

    private void Start() {
        Services.Container.Resolve<Wallet>().OnItemAmountChanged += DoInventoryBlink;
        var playerItemsTracker = Services.Container.Resolve<InventoryService>().PlayerProfile.playerItemsTracker;
        playerItemsTracker.OnListChange += RefreshRedDot;

        var hasNewItem = playerItemsTracker.ListOfNonVisualizedItems.Count >= 1 || playerItemsTracker.ListOfNotVisualizedSlots.Count >= 1;

        inventoryRedDot.SetActive(hasNewItem);
    }

    public void ToggleAutoFight()
    {
        autoFightEnabled = !autoFightEnabled;
        //autoFightButtonBackground.color = autoFightEnabled ? Color.green : Color.red;
    }
    
    public void UpdateInteractionNode(IInteractableNode node)
    {
        this.node = node;
        if (node == null)
        {
        }
        else if (node.PlayerBehaviour == typeof(ChopTreeBehaviour))
        {
            buttonLabel.text = "CHOP";
            buttonIcon.sprite = chopIcon;
        }
        else if (node.PlayerBehaviour == typeof(FightBehaviour))
        {
            buttonLabel.text = "FIGHT";
            buttonIcon.sprite = fightIcon;
        }
        else if (node.PlayerBehaviour == typeof(QuestTalkBehaviour))
        {
            buttonLabel.text = "TALK";
            buttonIcon.sprite = talkIcon;
        }
        else if (node.PlayerBehaviour == typeof(FishingBehaviour))
        {
            buttonLabel.text = "FISH";
            buttonIcon.sprite = fishIcon;
        }
        else
        {
            buttonLabel.text = "INTERACT";
            buttonIcon.sprite = talkIcon;
        }
        tween?.Kill();
        buttonCG.DOFade(node == null ? 0f : 1f, .15f);
        buttonCG.interactable = node != null;
        buttonCG.blocksRaycasts = node != null;
        tween = button.transform.DOScale(initialSizeDelta * ( node == null ? .8f : 1f), .2f);
        //autoFightButtonBackground.color = autoFightEnabled ? Color.green : Color.red;
    }

    public void RefreshRedDot()
    {
        var playerItemsTracker = Services.Container.Resolve<InventoryService>().PlayerProfile.playerItemsTracker;

        var hasNewItem = playerItemsTracker.ListOfNonVisualizedItems.Count >= 1 || playerItemsTracker.ListOfNotVisualizedSlots.Count >= 1;

        inventoryRedDot.SetActive(hasNewItem);
    }

    public void DoInventoryBlink(Dictionary<Guid, long> dictionary)
    {
        if (isInventoryBlinking) return;

        var fadeDuration = 0.5f;
        isInventoryBlinking = true;
        inventoryBlink.DOFade(1, fadeDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            inventoryBlink.DOFade(0, fadeDuration).SetEase(Ease.OutCubic).OnComplete(()=> isInventoryBlinking = false);
        });
    }
}