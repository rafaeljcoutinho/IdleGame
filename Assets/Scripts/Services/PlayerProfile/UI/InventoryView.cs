using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    public class ViewData {}
    
    [SerializeField] private GridLayoutSmartLayout gridLayoutSmartLayout;
    [SerializeField] private ItemIconPrefab itemViewPrefab;
    [SerializeField] private ScrollRect listItemsScrollRect;
    [SerializeField] private Transform listItemsPanel;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private TextMeshProUGUI coinsLabel;
    [SerializeField] private selectedView tabViewController;
    [SerializeField] private Image armorButton;
    [SerializeField] private Image materialsButton;
    [SerializeField] private Image consumableButton;
    [SerializeField] private Color showingColor;
    [SerializeField] private Color hideColor;
    [SerializeField] private TextMeshProUGUI pageText;

    private bool showArmor = true;
    private bool showMaterial = true;
    private bool showConsumable = true;
    private List<InventoryItemViewData> equipment = new();
    private List<InventoryItemViewData> material = new();
    private List<InventoryItemViewData> consumable = new();

    public Dictionary<string, int> inventoryViewItemsIdPosition = new();
    public List<ItemIconPrefab> prefabsPositionsList = new();

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


    public void Init()
    {
        ClearPrefabs();
        RefreshCoinsView();
        PopulateItems();
        PopulateView();
    }

    private void ClearPrefabs()
    {
        foreach (var prefab in prefabsPositionsList)
        {
            Destroy(prefab.gameObject);
        }
        prefabsPositionsList.Clear();
    }

    private void PopulateView()
    {
        inventoryViewItemsIdPosition.Clear();
        int currentIndex = 0;

        if (showArmor)
        {
            for (int i = 0; i < equipment.Count; i++)
            {
                var item = equipment[i];
                var prefab = InstantiateNewPrefab();
                TrySetData(item, prefab);
                inventoryViewItemsIdPosition.Add(item.itemData.id, i + currentIndex);
            }
        }
        if (showMaterial)
        {
            for (int i = 0; i < material.Count; i++)
            {
                var item = material[i];
                var prefab = InstantiateNewPrefab();
                TrySetData(item, prefab);
                inventoryViewItemsIdPosition.Add(item.itemData.id, i + currentIndex);
            }
        }
        if (showConsumable)
        {
            for (int i = 0; i < consumable.Count; i++)
            {
                var item = consumable[i];
                var prefab = InstantiateNewPrefab();
                TrySetData(item, prefab);
                inventoryViewItemsIdPosition.Add(item.itemData.id, i + currentIndex);
            }
        }

    }

    public void RefreshCoinsView()
    {
        coinsLabel.text = SkillExperienceTable.Format(Services.Container.Resolve<Wallet>().ItemAmount(Wallet.coinsId));
    }

    public ItemIconPrefab InstantiateNewPrefab()
    {
        var newPrefab = Instantiate(itemViewPrefab, listItemsPanel);
        prefabsPositionsList.Add(newPrefab);
        return newPrefab;
    }


    public void Redraw()
    {
        Init();
    }

    private void TrySetData(InventoryItemViewData inventoryItemViewData, ItemIconPrefab itemIconPrefab)
    {
        if (inventoryItemViewData == null)
        {
            prefabsPositionsList.Remove(itemIconPrefab);
            Destroy(itemIconPrefab.gameObject);
        }
        else
        {
            itemIconPrefab.SetData(inventoryItemViewData, true);
            itemIconPrefab.SetButton(OnItemClick);
        }
    }

    public void OnItemClick(InventoryItemViewData inventoryItemViewData, Vector3 position)
    {
        if(inventoryItemViewData.itemData is Equipment)
        {   
            tabViewController.OpenTabByItem(inventoryItemViewData.itemData as Equipment);
        }
        var prefabPos = inventoryViewItemsIdPosition[inventoryItemViewData.itemData.id];
        prefabsPositionsList[prefabPos].SetRedDot(false);

        OverlayCanvas.Instance.ToolTip.Show(inventoryItemViewData.itemData, position, ConsumableItemResolver.Source.Inventory);
    }

    private void PopulateItems()
    {
        var wallet = Services.Container.Resolve<Wallet>();
        var itemDataBase = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;
        consumable.Clear();
        equipment.Clear();
        material.Clear();
        foreach (var itemQuantity in wallet.Items)
        {
            if (itemQuantity.Value == 0) continue;
            
            var item = itemDataBase.GetItem(itemQuantity.Key);
            var itemViewData = new InventoryItemViewData{itemData = item, itemQuantity = itemQuantity.Value};

            if (item is IInventoryViewable)
            {
                if (item is IConsumable)
                {
                    consumable.Add(itemViewData);
                }
                else if (item is Equipment)
                {
                    equipment.Add(itemViewData);
                }
                else 
                {
                    material.Add(itemViewData);
                }
            }
        }
        consumable.Sort((item1, item2) => item1.itemData.Position.CompareTo(item2.itemData.Position));
        equipment.Sort((item1, item2) => item1.itemData.Position.CompareTo(item2.itemData.Position));
        material.Sort((item1, item2) => item1.itemData.Position.CompareTo(item2.itemData.Position));
    }


    public void OnArmorClick()
    {
        showArmor = !showArmor;
        armorButton.color = showArmor? showingColor : hideColor;
        Init();
    }
    public void OnMaterialClick()
    {
        showMaterial = !showMaterial;
        materialsButton.color = showMaterial? showingColor : hideColor;
        Init();
    }
    public void OnConsumableClick()
    {
        showConsumable = !showConsumable;
        consumableButton.color = showConsumable? showingColor : hideColor;
        Init();
    }
}


public class InventoryItemViewData
{
    public Item itemData;
    public long itemQuantity;
}