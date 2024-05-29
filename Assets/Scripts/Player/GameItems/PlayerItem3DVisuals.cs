using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerItem3DVisuals : MonoBehaviour
{
    [Serializable]
    public class Slot
    {
        public Equipment.EquipSlot visualSlot;
        public Transform Transform;
    }

    [SerializeField] private Player player;
    [SerializeField] private List<Slot> Slots;
    [SerializeField] private EquipmentBehaviour defaultBehavior;
    [SerializeField] private Transform defaultPrefabParent;
    private Dictionary<Guid, EquipmentBehaviour> gameItemInstances;

    private void Start()
    {
        gameItemInstances = new();
        ResetVisualsDefaults();
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged += OnEquipmentChanged;
    }

    private void OnDestroy()
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged -= OnEquipmentChanged;
    }

    public EquipmentBehaviour GetBehavior(Equipment equipment)
    {
        if (gameItemInstances.ContainsKey(equipment.Uuid))
        {
            return gameItemInstances[equipment.Uuid];
        }

        return defaultBehavior;
    }

    private void OnEquipmentChanged(PlayerEquipmentManager.EquippedItemsChangedArgs obj)
    {
        
        var itemDatbase =Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;
        if (obj.UnequippedItem != null)
        {
            var equippedItem = itemDatbase.GetItem(obj.UnequippedItem.Id);
            if (equippedItem == null)
                return;
            if (equippedItem is Equipment equipment && !PlayerEquipmentManager.FoodSlots.Contains(equipment.Slot))
            {

                Disable(equipment);
            }
        }
        
        if (obj.EquippedItem != null)
        {
            var equippedItem = itemDatbase.GetItem(obj.EquippedItem.Id);
            if (equippedItem == null)
                return;
            if (equippedItem is Equipment equipment && !PlayerEquipmentManager.FoodSlots.Contains(equipment.Slot))
            {
                Enable(equipment);
            }
        }
    }
    public void ResetVisualsDefaults()
    {
        var equippedItems = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.EquippedItems;
        var itemDatabase = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;

        foreach (var kvPair in equippedItems)
        {
            var itemId = kvPair.Value.Id;
            var equipment = itemDatabase.GetItem(itemId) as Equipment;
            if (PlayerEquipmentManager.ToolbeltSlots.Contains(equipment.Slot))
            {
                Disable(equipment);
            }
            else if (!PlayerEquipmentManager.FoodSlots.Contains(equipment.Slot))
            {
                Enable(equipment);
            }
        }
    }

    public void Disable(Equipment equipment)
    {
        if (gameItemInstances.ContainsKey(equipment.Uuid))
        {
            gameItemInstances[equipment.Uuid].Disable();
        }
    }
    
    public void Enable(Equipment equipment)
    {
        if (equipment.ItemPrefab == null)
        {
            return;
        }
        EquipmentBehaviour newInstance = null;
        if (gameItemInstances.ContainsKey(equipment.Uuid))
        {
            newInstance = gameItemInstances[equipment.Uuid];
        }
        else
        {
            var found = false;
            foreach (var t in Slots)
            {
                if (t.visualSlot != equipment.Slot) 
                    continue;
                found = true;
                newInstance = Instantiate(equipment.ItemPrefab, t.Transform);
                newInstance.SetPlayer(player);
                gameItemInstances.Add(equipment.Uuid, newInstance);
            }

            if (!found)
            {
                newInstance = Instantiate(equipment.ItemPrefab, defaultPrefabParent);
                newInstance.SetPlayer(player);
                gameItemInstances.Add(equipment.Uuid, newInstance);
            }
        }
        newInstance.Enable();
    }
    
    public void StartPlayerBehavior()
    {
        Services.Container.Resolve<DamageProcessorService>().StartDamageTick();
    }

    public void TakeDamage(HitInfo damageInfo)
    {
        var itemDatabase = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;
        foreach (var behavior in gameItemInstances)
        {
            if (!behavior.Value.Enabled)
            {
                continue;
            }

            behavior.Value.Bind(itemDatabase.GetItem(behavior.Key) as Equipment);
            behavior.Value.DamageTaken(damageInfo);
        }
    }

    public void FinishPlayerBehavior(float dt, GameplaySceneBootstrapper gameplaySceneBootstrapper)
    {
        var itemDatabase = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;
        foreach (var behavior in gameItemInstances)
        {
            if (!behavior.Value.Enabled)
            {
                continue;
            }

            behavior.Value.Bind(itemDatabase.GetItem(behavior.Key) as Equipment);
            var enemies = new List<Enemy>();
            foreach (var enemySpawner in gameplaySceneBootstrapper.EnemySpawner)
            {
                enemies.AddRange(enemySpawner.Enemies);
            }
            behavior.Value.SetEnemies(enemies);
            behavior.Value.UpdateBehavior(dt);
        }

        var damageProcessor = Services.Container.Resolve<DamageProcessorService>();
        damageProcessor.ProcessDamagePool();

        var listOfDamages = damageProcessor.DamageInfoPool.InUse.ToList();
        foreach (var behavior in gameItemInstances)
        {
            if (!behavior.Value.Enabled)
            {
                continue;
            }
            foreach (var processedDamage in listOfDamages)
            {
                behavior.Value.PostDamage(processedDamage);
            }
        }

        damageProcessor.Return(listOfDamages);
        damageProcessor.ProcessDamagePool();
    }
}
