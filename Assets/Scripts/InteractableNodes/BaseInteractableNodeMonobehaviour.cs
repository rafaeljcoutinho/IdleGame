using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseInteractableNodeMonobehaviour : MonoBehaviour, IInteractableNode
{
    public abstract NodeData NodeData { get; }
    [SerializeField] protected Outline outline;
    [SerializeField] protected OutlineManager outlineManager;
    [SerializeField] protected Color outlineColor;
    [SerializeField] protected Transform outlineTransform;
    public virtual Type PlayerBehaviour { get; }

    public virtual void Highlight()
    {
        if (outlineManager != null)
        {
            outlineManager.Show(outlineTransform, outlineColor);
        }
        
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    public virtual void Interact() { }

    public virtual void DeHighlight()
    {
        if (outlineManager != null)
        {
            outlineManager.Hide();
        }
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    public void DropFromDroptable(SkillData.Type type, EnemyHpController hpController, ActionNodeData actionNodeData,
        Transform dropPosition)
    {
        var itemDrops = GetDropsWithOverkill(hpController, actionNodeData);
        itemDrops = Services.Container.Resolve<SkillService>().ScaleXpDrops(itemDrops, actionNodeData, type);

        GiveItemsToPlayer(itemDrops, dropPosition);
    }

    public void DropFromDroptable(SkillData.Type type, float dropCount, ActionNodeData actionNodeData,
        Transform dropPosition, Dictionary<SkillData.Type, float> damageTypesPercentage)
    {
        var itemDrops = Services.Container.Resolve<DropGeneratorService>().GenerateDrops(actionNodeData.droptable, dropCount);

        var keys = itemDrops.Keys.ToList();
        foreach (Guid key in keys)
        {
            var item = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(key);
            if (item is Experience)
            {
                var TotalCharacterXp = itemDrops[key];

                var skillsDamage = damageTypesPercentage.Keys.ToList();
                foreach (SkillData.Type skill in skillsDamage)
                {
                    var xpAmount = damageTypesPercentage[skill] * TotalCharacterXp * 1.2f;
                    var xpItem = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(skill).XpItem.Uuid;
                    itemDrops.Add(xpItem, (int)xpAmount);
                }
            }
        }

        itemDrops = Services.Container.Resolve<SkillService>().ScaleXpDrops(itemDrops, actionNodeData, type);
        GiveItemsToPlayer(itemDrops, dropPosition);
    }

    private Dictionary<Guid, long> GetDropsWithOverkill(EnemyHpController hpController, ActionNodeData actionNodeData)
    {
        var overkillRecord = Services.Container.Resolve<OverkillService>().GetOverkillRecord(actionNodeData.Uuid);
        return Services.Container.Resolve<DropGeneratorService>().GenerateDrops(actionNodeData.droptable, overkillRecord != null ? overkillRecord.overkillInfo.OverkillLogarithim : 1);
    }

    protected void GiveItemsToPlayer(Dictionary<Guid, long> itemDrops, Transform dropPosition)
    {
        var inventory = Services.Container.Resolve<InventoryService>();
        inventory.PlayerProfile.Wallet.GiveItems(itemDrops);
        OverlayCanvas.Instance.ShowDrops(itemDrops);
        ShowItemDrops(dropPosition, itemDrops);
        inventory.Save();
    }

    private void ShowItemDrops(Transform dropPosition, Dictionary<Guid, long> itemDrops)
    {
        return;
        int i = 0;
        var coroutineDispatcher = Services.Container.Resolve<CoroutineDispatcher>();
        foreach (var item in itemDrops.Keys.ToList())
        {
            var itemToShow = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.GetItem(item);
            if (itemToShow is Experience)
            {
                continue;
            }

            var damageNumberView = OverlayCanvas.Instance.DisposableViewPool.Get<DamageNumberView>();
            var itemRarityColor = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.RarityColorPairs[(int)itemToShow.rarity].color;
            int j = i;
            coroutineDispatcher.AfterDelay(.3f, () =>
            {
                damageNumberView.ShowSimple(new DamageNumberView.ViewData
                {
                    color = itemRarityColor,
                    text = $"<size=50%>{itemDrops[item]}x {itemToShow.name}",
                    spread = 0f,
                }, dropPosition, Vector3.up * -(1 + j) * .5f);
                OverlayCanvas.Instance.DisposableViewPool.Return(damageNumberView, 2f);
            });
            i++;
        }
    }
}