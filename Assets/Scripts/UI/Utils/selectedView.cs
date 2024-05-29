using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class selectedView : MonoBehaviour
{
    [SerializeField] private TabView weapons;
    [SerializeField] private TabView armor;
    [SerializeField] private TabView food;
    [SerializeField] private TabView toolbelt;
    [SerializeField] private GameObject weaponsRedDot;
    [SerializeField] private GameObject armorRedDot;
    [SerializeField] private GameObject foodsRedDot;
    [SerializeField] private GameObject toolsRedDot;

    [SerializeField] private StatsQuickView statsQuickView;

    private bool loaded = false;
    private List<TabView> TabView = new();
    private Dictionary<TabView, int> tabIndex = new();
    private PlayerEquipmentManager equipmentManager;

    public int selectedIndex { get; private set; }

    private void OnEnable() {

        if (!loaded) equipmentManager = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager;
        
        armorRedDot.SetActive(false);
        foodsRedDot.SetActive(false);
        toolsRedDot.SetActive(false);
        weaponsRedDot.SetActive(false);

        var playerItemsTracker = Services.Container.Resolve<InventoryService>().PlayerProfile.playerItemsTracker;
        foreach (var itemSlot in playerItemsTracker.ListOfNotVisualizedSlots)
        {
            if (IsWeaponSlot(itemSlot))
            {
                weaponsRedDot.SetActive(true);
                continue;
            }
            else if (equipmentManager.IsFoodSlot(itemSlot))
            {
                foodsRedDot.SetActive(true);
                continue;
            }
            else if (equipmentManager.IsToolbeltSlot(itemSlot))
            {
                toolsRedDot.SetActive(true);
                continue;
            }
            else 
            {
                armorRedDot.SetActive(true);
                continue;
            }
        }

        if(loaded) 
            return;


        TabView.Add(weapons);
        TabView.Add(armor);
        TabView.Add(food);
        TabView.Add(toolbelt);

        for(int i = 0; i < TabView.Count; i++)
        {
            var j = i;
            tabIndex.Add(TabView[i], i);
            TabView[i].button.onClick.AddListener(() => OpenView(j));
        }

        loaded = true;
    }

    public bool IsWeaponTabOpen => selectedIndex == 0;
    public bool IsArmorTabOpen => selectedIndex == 2;
    public bool IsFoodTabOpen => selectedIndex == 2;
    public bool IsToolbeltTabOpen => selectedIndex == 3;

    public void OnStatsViewClick()
    {
        if (statsQuickView.IsShowing)
        {
            statsQuickView.Hide();
        }
        else
        {
            if (IsToolbeltTabOpen)
            {
                statsQuickView.ShowSkills();
            }
            else
            {
                statsQuickView.ShowDamage();
            }
        }
    }

    private bool IsWeaponSlot(Equipment.EquipSlot slot)
    {
        foreach (var weaponSlot in WeaponSlots)
        {
            if (slot == weaponSlot) return true;
        }
        return false;
    }

    private List<Equipment.EquipSlot> WeaponSlots = new()
    {
        Equipment.EquipSlot.MainHand, Equipment.EquipSlot.OffHand, Equipment.EquipSlot.Ring,
        Equipment.EquipSlot.Necklace, Equipment.EquipSlot.Bracelet,
    };
    public void OpenTabByItem(Equipment item)
    {

        if(equipmentManager.IsFoodSlot(item.Slot))
        {
            OpenView(tabIndex[food]);
        }
        else if(equipmentManager.IsToolbeltSlot(item.Slot))
        {
            OpenView(tabIndex[toolbelt]);
        }
        else if(WeaponSlots.Contains(item.Slot))
        {
            OpenView(tabIndex[weapons]);
        }
        else
        {
            OpenView(tabIndex[armor]);
        }
    }

    private void RemoveRedDot(int index)
    {
        var playerItemsTracker = Services.Container.Resolve<InventoryService>().PlayerProfile.playerItemsTracker;

        if (index == 0)
        {
            weaponsRedDot.SetActive(false);
            foreach (var weaponSlot in WeaponSlots)
            {
                playerItemsTracker.VisualizeSlot(weaponSlot);
            }
        }
        else if (index == 1)
        {
            armorRedDot.SetActive(false);
            foreach (var equipSlot in PlayerEquipmentManager.EquipmentSlotsOnly)
            {
                playerItemsTracker.VisualizeSlot(equipSlot);
            }
        }
        else if (index == 2)
        {
            foodsRedDot.SetActive(false);
            foreach (var foodSlot in PlayerEquipmentManager.FoodSlots)
            {
                playerItemsTracker.VisualizeSlot(foodSlot);
            }
        }
        else if (index == 3)
        {
            toolsRedDot.SetActive(false);
            foreach (var toolSlot in PlayerEquipmentManager.ToolbeltSlots)
            {
                playerItemsTracker.VisualizeSlot(toolSlot);
            }
        }

    }
    


    public void OpenView(int index)
    {
        selectedIndex = index;
        for(int i = 0; i < TabView.Count; i++)
        {
            if(i == index)
            {
                TabView[i].SetViewActive(true);
                RemoveRedDot(index);
            }
            else
            {
                TabView[i].SetViewActive(false);
            }
        }
        if (statsQuickView.IsShowing)
        {
            if (IsToolbeltTabOpen)
            {
                statsQuickView.ShowSkills();
            }
            else
            {
                statsQuickView.ShowDamage();
            }   
        }
    }
}

[Serializable]
public class TabView
{
    [SerializeField] public Button button;
    [SerializeField] public List<GameObject> view;

    public void SetViewActive(bool isActive){
        foreach(var go in view)
        {
            go.SetActive(isActive);
        }
    }

}