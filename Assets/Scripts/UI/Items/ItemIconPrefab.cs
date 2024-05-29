using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EquipmentView;

public class ItemIconPrefab : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject levelContainer;
    [SerializeField] private List<Image> levelColorObjects;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI itemQuantityText;
    [SerializeField] private Button button;
    [SerializeField] private Button claimButton;
    [SerializeField] private List<GameObject> allUIelements;
    [SerializeField] private Image rarity;
    [SerializeField] private TextMeshProUGUI itemPriceLabel;
    [SerializeField] private Image itemPrice;
    [SerializeField] private GameObject discountContainer;
    [SerializeField] private GameObject freeContainer;
    [SerializeField] private GameObject priceContainer;
    [SerializeField] private GameObject claimContainer;
    [SerializeField] private TextMeshProUGUI discountLabel;
    [SerializeField] private GameObject blocker;
    [SerializeField] private GameObject emptyMessage;
    [SerializeField] private GameObject redDot;

    private const string LEVEL_TEXT = "<size=75%> Lv. </size>{0}";
    private const string QUANTITY_TEXT = "x{0}";

    private InventoryItemViewData inventoryItemViewData;
    public InventoryItemViewData InventoryItemViewData => inventoryItemViewData;
    private StoreItemViewData storeItemViewData;

    public void SetData(InventoryItemViewData inventoryItemViewData, bool useRedDot = false){
        CleanData();
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = inventoryItemViewData.itemData.SmallThumbnail;
        itemQuantityText.text = string.Format(QUANTITY_TEXT,SkillExperienceTable.Format(inventoryItemViewData.itemQuantity));
        var itemRarity = inventoryItemViewData.itemData.rarity;
        rarity.color = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.RarityColorPairs[(int)itemRarity].color;
        SetLevelColor(rarity.color);

        if (inventoryItemViewData.itemData is Equipment equip && !PlayerEquipmentManager.FoodSlots.Contains(equip.Slot))
        {
            SetItemLevel(equip.ItemLv);
        }

        if (useRedDot)
        {
            var isItemNew = Services.Container.Resolve<InventoryService>().PlayerProfile.playerItemsTracker.ListOfNonVisualizedItems.Contains(inventoryItemViewData.itemData.Uuid);
            SetRedDot(isItemNew);
            if (isItemNew) Services.Container.Resolve<InventoryService>().PlayerProfile.playerItemsTracker.VisualizeItem(inventoryItemViewData.itemData.Uuid);
        }

        foreach(var go in allUIelements){
            go.SetActive(true);
        }
        
        this.inventoryItemViewData = inventoryItemViewData;
    }

    public void SetRedDot(bool isEnable)
    {
        redDot.SetActive(isEnable);
    }

    public void SetItemLevel(int level)
    {
        levelContainer.SetActive(true);
        levelText.text = string.Format(LEVEL_TEXT, level);
    }
    public void SetLevelEnabled(bool isEnable)
    {
        levelContainer.SetActive(isEnable);
    }

    public void SetLevelColor(Color color)
    {
        foreach(var go in levelColorObjects)
        {
            go.color = color;
        }
    }

    public void SetStoreData(StoreItemViewData storeItemViewData)
    {
        this.storeItemViewData = storeItemViewData;
        itemPriceLabel.text = SkillExperienceTable.Format(storeItemViewData.price.quantity);
        if (storeItemViewData.quantityEnabled == 0)
        {
            levelContainer.SetActive(false);
            emptyMessage.SetActive(true);
            blocker.SetActive(true);
            button.onClick.RemoveAllListeners();
            priceContainer.SetActive(false);
            discountContainer.SetActive(false);
            claimContainer.SetActive(false);
            freeContainer.SetActive(false);
        }
        
        if (storeItemViewData.price.item != null)
            itemPrice.sprite = storeItemViewData.price.item.SmallThumbnail;

        itemPriceLabel.gameObject.SetActive(true);
    }
    public void SetQuantity(string quantity){
        itemQuantityText.text = quantity;
    }

    public void EnableDiscount(int amount)
    {
        discountLabel.text = amount.ToString() + "%";
        discountContainer.SetActive(true);
    }

    public void EnableFree(Action<StoreItemViewData> action)
    {
        claimButton.onClick.AddListener(()=> action(storeItemViewData));
        discountContainer.SetActive(false);
        freeContainer.SetActive(true);
        priceContainer.SetActive(false);
        claimContainer.SetActive(true);
    }

    public void CleanData(){
        button.onClick.RemoveAllListeners();
        SetRedDot(false);
        levelContainer.SetActive(false);
        foreach(var go in allUIelements){
            go.SetActive(false);
        }
    }


    public void SetButton(Action<InventoryItemViewData, Vector3> action){
        button.onClick.AddListener(()=> action(inventoryItemViewData, transform.position));
    }

    public void SetButton(Action<StoreItemViewData, Vector3> action){
        button.onClick.AddListener(()=> action(storeItemViewData, transform.position));
    }
    public void SetButton(Action<ItemSlotPair> action, ItemSlotPair itemSlotPair){
        
        button.onClick.AddListener(()=> 
            {
                action(itemSlotPair);
            });
    }

    public void ClearEquipItemData()
    {
        itemImage.gameObject.SetActive(false);
        rarity.color = Color.white;
        itemQuantityText.text = "";
        SetLevelEnabled(false);
        button.onClick.RemoveAllListeners();
    }

}
