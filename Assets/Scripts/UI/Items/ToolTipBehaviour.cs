
using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject statsHolder;
    [SerializeField] private TextMeshProUGUI notEnoughLevelLabel; 
    [SerializeField] private Button cannotEquipButton;
    [SerializeField] private ItemIconPrefab itemIconPrefab;
    [SerializeField] private GameObject buttonHolder;
    [SerializeField] private Button leftButton;
    [SerializeField] private TextMeshProUGUI leftButtontText;
    [SerializeField] private List<Button> rightButton;
    [SerializeField] private List<TextMeshProUGUI> rightButtonText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private List<TextMeshProUGUI> statsNameLabel;
    [SerializeField] private List<TextMeshProUGUI> statsValueLabel;
    [SerializeField] private TextMeshProUGUI itemType;
    [SerializeField] private ActionTooltipSection actionTooltipSection;

    [SerializeField] private GameObject blocker;    

    [SerializeField] private List<Image> rarityImages;
    [SerializeField] private TextMeshProUGUI itemName;

    private const string CONCAT_TYPE = "{0} / {1}";
    private int buttonIndex;
    private bool isShowingCannotEquipText;

    private const string REMOVE_BUTTON_TEXT = "REMOVE";
    private const string EQUIPPED_BUTTON_TEXT = "EQUIPPED";
    private const string SELL_BUTTON_TEXT = "SELL";
    private const string USE_BUTTON_TEXT = "USE";
    private const string EQUIP_BUTTON_TEXT = "EQUIP";
    private const string CONSUMABLE_TEXT = "consumable";
    private const string COMMOM_TEXT = "commom ";
    private const string UNCOMMOM_TEXT = "uncommom ";
    private const string RARE_TEXT = "rare ";
    private const string EPIC_TEXT = "epic ";
    private const string LEGENDARY_TEXT = "legendary ";
    private const string MATERIAL_TEXT = "material";
    private const string EQUIPMENT_TEXT = "equipment";
    private const string CANNOT_EQUIP_FOOD_TEXT = "need one empty food slot to do this";
    private Item item;
    private Sequence tween;
    public void Show(Item item, Vector3 position, ConsumableItemResolver.Source source, bool disableButtons = false)
    {
        this.item = item;
        buttonIndex = 0;
        transform.position = position;
        var rarityColor = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.RarityColorPairs[(int)item.rarity].color;
        var itemNameRarityColor = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.ItemNameRarityColorPairs[(int)item.rarity].color;
        leftButtontText.text = SELL_BUTTON_TEXT;
        itemIconPrefab.SetLevelEnabled(false);
        cannotEquipButton.gameObject.SetActive(false);

        itemName.text = item.LocalizationKey;
        description.text = item.DescriptionLocalizationKey;
        itemIcon.sprite = item.SmallThumbnail;
        itemType.text = "";

        foreach(var button in rightButton)
        {
            button.interactable = true;
            SetButtonDisabledColorAlpha(button, 0);
        }
        leftButton.interactable = true;

        blocker.SetActive(true);

        foreach(var image in rarityImages){
            image.color = rarityColor;
        }
        itemName.color = itemNameRarityColor;


        if(item is not IConsumable && item is not Equipment && item is not ISellable){
            buttonHolder.SetActive(false);
        }else{
            buttonHolder.SetActive(true);
        }
        
        if(item is IConsumable)
        {
            SetAction(() => TryUseItem(source,item), rightButton[buttonIndex]);
            rightButton[buttonIndex].gameObject.SetActive(true);
            rightButtonText[buttonIndex].text = USE_BUTTON_TEXT;
            buttonIndex++;
            if(itemType.text == "")
            {
                itemType.text = CONSUMABLE_TEXT;
            }
            else
            {
                itemType.text = string.Format(CONCAT_TYPE, itemType.text, CONSUMABLE_TEXT);
            }
        }
        if(item is ISellable)
        {
            SetAction(() => TrySellItem(item), leftButton);
            leftButton.gameObject.SetActive(true);
        }
        if(item is MonsterDrops)
        {
            if(itemType.text == "")
            {
                itemType.text = MATERIAL_TEXT;
            }
            else
            {
                itemType.text = string.Format(CONCAT_TYPE, itemType.text, MATERIAL_TEXT);
            }
        }
        if(item is Equipment equipItem)
        {
            SetAction(() => TryEquipItem(item), rightButton[buttonIndex]);
            var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
            var itemEquipped = profile.PlayerEquipmentManager.GetItemOnSlot(equipItem.Slot);

            if(item == itemEquipped && (!PlayerEquipmentManager.FoodSlots.Contains(equipItem.Slot)))
            {
                rightButton[buttonIndex].gameObject.SetActive(true);
                rightButton[buttonIndex].interactable = false;
                rightButtonText[buttonIndex].text = EQUIPPED_BUTTON_TEXT;
            }
            else
            {
                rightButton[buttonIndex].gameObject.SetActive(true);
                rightButton[buttonIndex].interactable = true;
                rightButtonText[buttonIndex].text = EQUIP_BUTTON_TEXT;
            }
            notEnoughLevelLabel.transform.position = rightButton[buttonIndex].transform.position + Vector3.up;

            if (!PlayerEquipmentManager.FoodSlots.Contains(equipItem.Slot))
            {
                itemIconPrefab.SetItemLevel(equipItem.ItemLv);
                itemIconPrefab.SetLevelColor(rarityColor);
                var PlayerEquipmentManager = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager;
                if (!PlayerEquipmentManager.CanEquipItem(equipItem.Uuid, equipItem.Slot, 1) && equipItem != PlayerEquipmentManager.GetItemOnSlot(equipItem.Slot))
                {
                    cannotEquipButton.transform.position = rightButton[buttonIndex].transform.position;
                    cannotEquipButton.gameObject.SetActive(true);
                }
            }
            
            if(itemType.text == "")
            {
                itemType.text = EQUIPMENT_TEXT;
            }
            else
            {
                itemType.text = string.Format(CONCAT_TYPE, itemType.text, EQUIPMENT_TEXT);
            }
        }

        itemType.text = GetRarityText(item.rarity) + itemType.text;

        // handle stats
        if (item is Equipment equip)
        {
            var otherEquippedModifier = Services.Container.Resolve<InventoryService>().PlayerProfile
                .PlayerEquipmentManager.GetItemOnSlot(equip.Slot);
            SetModifiers(equip.Modifiers, (otherEquippedModifier as Equipment)?.Modifiers);
        } else if (item is IEquipmentModifier equipmentModifier)
        {
            SetModifiers(equipmentModifier.Modifiers, null);
        } else
        {
            statsHolder.SetActive(false);
        }

        if (disableButtons)
        {
            foreach(var button in rightButton)
            {
                button.gameObject.SetActive(false);
            }
            leftButton.gameObject.SetActive(false);
            buttonHolder.SetActive(false);
        }

        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        buttonIndex = 0;
    }

    private void EndTween()
    {
        tween?.Kill();
        isShowingCannotEquipText = false;
        notEnoughLevelLabel.alpha = 0;
    }

    public void ShowCannotEquipText()
    {
        if (isShowingCannotEquipText) return;

        var initialPosition = (cannotEquipButton.transform as RectTransform).anchoredPosition.y;
        var rectTransform = notEnoughLevelLabel.transform as RectTransform;

        notEnoughLevelLabel.alpha = 0;
        notEnoughLevelLabel.gameObject.SetActive(true);
        var temp = rectTransform.anchoredPosition;
        temp.y = initialPosition;
        rectTransform.anchoredPosition = temp; 
        isShowingCannotEquipText = true;

        tween?.Kill();
        tween = DOTween.Sequence();

        tween.Join(notEnoughLevelLabel.DOFade(1, .3f));
        tween.Join(rectTransform.DOAnchorPosY(initialPosition + rectTransform.rect.height * 2f, .4f).SetEase(Ease.OutCubic))
            .Append(notEnoughLevelLabel.DOFade(0, .3f).SetDelay(1f))
            .OnComplete(()=> {
                    var temp = rectTransform.anchoredPosition;
                    temp.y = initialPosition;
                    rectTransform.anchoredPosition = temp; 
                    notEnoughLevelLabel.gameObject.SetActive(false);
                    isShowingCannotEquipText = false;
                });
    }
    private void SetButtonDisabledColorAlpha(Button button,float a)
    {
        var colors = button.colors;
        var disableColor = colors.disabledColor;
        disableColor.a = a;
        colors.disabledColor = disableColor;
        button.colors = colors; 
    }
    private string GetRarityText(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Commom: 
                return COMMOM_TEXT;
            case Rarity.Uncommom: 
                return UNCOMMOM_TEXT;
            case Rarity.Rare: 
                return RARE_TEXT;
            case Rarity.Epic: 
                return EPIC_TEXT;
            case Rarity.Legendary: 
                return LEGENDARY_TEXT;
        }
        return "";
    }

    public const string StatNamePositiveComparison = "{0} <color=#70C053>(+{1})</color>";
    public const string StatNameNegativeComparison = "{0} <color=#EC5858>({1})</color>";
    public const string PositiveColor = "#70C053";
    public const string NegativeColor = "#EC5858";
    void SetModifiers(ActionModifiers myModifiers, ActionModifiers otherModifiers)
    {
        var j = 0;
        if (otherModifiers == null)
        {
            var prettyModifierInfo = SkillData.GetPrettyModifier(myModifiers);
            for (var i = 0; i < prettyModifierInfo.Count; i++)
            {
                statsNameLabel[j].gameObject.SetActive(true);
                statsValueLabel[j].gameObject.SetActive(true);
                statsNameLabel[j].text = string.Format(StatNamePositiveComparison, prettyModifierInfo[i].Name, prettyModifierInfo[i].FormattedValue);;
                statsValueLabel[j].text = prettyModifierInfo[i].FormattedValue;
                j++;
            }
        }
        else
        {
            var prettyModifierInfo = SkillData.GetPrettyModifierComparison(myModifiers, otherModifiers);
            for (var i = 0; i < prettyModifierInfo.Count; i++)
            {
                var format = prettyModifierInfo[i].Delta > 0 ? StatNamePositiveComparison : StatNameNegativeComparison;
                if (prettyModifierInfo[i].Delta != 0)
                {
                    statsNameLabel[j].text = string.Format(format, prettyModifierInfo[i].Name, prettyModifierInfo[i].DeltaFormatted);   
                }
                else
                {
                    statsNameLabel[j].text = prettyModifierInfo[i].Name;
                }
                statsNameLabel[j].gameObject.SetActive(true);
                statsValueLabel[j].gameObject.SetActive(prettyModifierInfo[i].Value != 0);
                statsValueLabel[j].text = prettyModifierInfo[i].FormattedValue;
                j++;
            }
        }

        statsHolder.SetActive(j > 0);
        
        for (var i = j; i < statsNameLabel.Count; i++)
        {
            statsNameLabel[i].gameObject.SetActive(false);
            statsValueLabel[i].gameObject.SetActive(false);
        }
    }
    

    public void SetAction(Action action, Button button){
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action?.Invoke());
    }

    private void TryEquipItem(Item item)
    {
        var equipment = item as Equipment;
        var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
        var checkResponse = profile.MeetsRequirement(equipment.Requirements);

        var slotToEquip = equipment.Slot;
        long quantityToEquip = 1;

        if(item is EquipableFood)
        {
            quantityToEquip = Services.Container.Resolve<Wallet>().Items[item.Uuid];
            var equippedFoodSlot = profile.PlayerEquipmentManager.FindFoodEquipSlot(item as EquipableFood);
            if(equippedFoodSlot != null)
            {
                slotToEquip = equippedFoodSlot.Value;
            }
            else
            {
                var slotFounded = false;
                foreach(var slot in PlayerEquipmentManager.FoodSlots)
                {
                    var itemEquippedOnSlot = profile.PlayerEquipmentManager.GetItemOnSlot(slot);
                    if(itemEquippedOnSlot == null){
                        slotToEquip = slot;
                        slotFounded = true;
                        break;
                    }
                }
                if(!slotFounded)
                {
                    OverlayCanvas.Instance.ToastNotificationContainer.Show(new TextLayout.LayoutData
                    {
                        LocalizedText = CANNOT_EQUIP_FOOD_TEXT
                    });
                    return;
                }
            }
        }
        if (!checkResponse.HasRequirements)
        {
            OverlayCanvas.Instance.ToastNotificationContainer.Show(new TextLayout.LayoutData
            {
                LocalizedText = checkResponse.LocalizedReasonForFail,
            });
            return;
        }
        var itemEquipped = profile.PlayerEquipmentManager.GetItemOnSlot(slotToEquip);
        bool sameItem = false;
        if (itemEquipped != null && itemEquipped == item && !PlayerEquipmentManager.FoodSlots.Contains(equipment.Slot))
        {   
            sameItem = true;
        }
        // no need to redraw, this will cascade a wallet change that already redraws
        if (!sameItem && profile.PlayerEquipmentManager.EquipItem(equipment.Uuid, slotToEquip, quantityToEquip))
        {
            Services.Container.Resolve<InventoryService>().Save();
        }
        Hide();
    }    
    private void TrySellItem(Item item)
    {
        var wallet = Services.Container.Resolve<Wallet>();
        var totalItemQuantity = wallet.ItemAmount(item.Uuid);
        actionTooltipSection.SetActionType(ActionType.Sell);
        var price = (item as ISellable).Cost;
        
        ShowQuantitySelector(price, totalItemQuantity, totalItemQuantity, ActionType.Sell);
    }

    Action<long> actionTooltipCallback;
    public void ShowQuantitySelector(ItemWithQuantity priceItem, long totalQuantity, long realQuantity, ActionType actionType, Action<long> callBack = null)
    {
        actionTooltipCallback = callBack;
        if (actionType == ActionType.Sell)
        {
            actionTooltipSection.SetActionType(ActionType.Sell);
            actionTooltipSection.Show( priceItem, totalQuantity, realQuantity, SellItem, SetButtonsEnable );
        }
        else if (actionType == ActionType.Buy)
        {
            actionTooltipSection.SetActionType(ActionType.Buy);
            actionTooltipSection.Show( priceItem, totalQuantity, realQuantity, BuyItem, SetButtonsEnable, false );
        }
        else if (actionType == ActionType.Use)
        {
            actionTooltipSection.SetActionType(ActionType.Use);
        }
    }

    private void BuyItem(ItemWithQuantity itemPrice, long amount)
    {
        actionTooltipCallback?.Invoke(amount);
        Hide();
    }

    private void SetButtonsEnable(bool isEnable)
    {
        buttonHolder.SetActive(isEnable);
    }

    public void SetEquipedItemButton(Action action)
    {
        buttonHolder.SetActive(true);
        leftButtontText.text = REMOVE_BUTTON_TEXT;
        leftButton.gameObject.SetActive(true);
        leftButton.onClick.RemoveAllListeners();
        leftButton.onClick.AddListener(()=> {action(); Hide();});
    }
    private void SellItem(ItemWithQuantity priceItem, long amount){
        var itemPrice = (item as ISellable).Cost.quantity;
        var wallet = Services.Container.Resolve<Wallet>();
        wallet.SpendItem(item.Uuid, amount);
        wallet.GiveItem(priceItem.item.Uuid, amount * itemPrice);
        Hide();
    }


    private void TryUseItem(ConsumableItemResolver.Source source, Item item)
    {
        item.Accept(Services.Container.Resolve<ConsumableItemResolver>().SetContext(OverlayCanvas.Instance.GameplaySceneBootstrapper.Player, source));
        Hide();
    }


    public void Hide(){
        EndTween();
        leftButton.gameObject.SetActive(false);
        actionTooltipSection.Hide();

        for(int i = 0; i < rightButton.Count; i++)
        {
            rightButton[i].gameObject.SetActive(false);
        }

        buttonHolder.SetActive(false);
        statsHolder.SetActive(false);
        gameObject.SetActive(false);
        blocker.SetActive(false);
    }
    
}