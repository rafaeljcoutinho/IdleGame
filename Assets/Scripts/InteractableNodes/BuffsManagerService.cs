using System;
using System.Collections.Generic;

public class BuffsManagerService
{
    private HashSet<Buff> activeBuffs;
    private ActionModifiers cachedSkillbuffModifier = new ();
    
    public BuffsManagerService()
    {
        var wallet = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet;
        wallet.OnItemAmountChanged += WalletOnOnItemAmountChanged;
        var itemDatabase = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;
        activeBuffs = new HashSet<Buff>();
        foreach (var itemWithAmount  in wallet.Items)
        {
            var id = itemWithAmount.Key;
            var item = itemDatabase.GetItem(id);
            if (item is Buff buff && itemWithAmount.Value > 0)
            {
                activeBuffs.Add(buff);
            }
        }
    }
    public ActionModifiers GetCombinedBuffs()
    {
        cachedSkillbuffModifier.Reset();
        foreach (var buff in activeBuffs)
        {
            if (buff is SkillBuff skillBuff)
            {
                cachedSkillbuffModifier.Combine(skillBuff.modifiers);
            }
        }
        return cachedSkillbuffModifier;
    }

    private void WalletOnOnItemAmountChanged(Dictionary<Guid, long> itemsChanged)
    {
        var itemDatabase = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;
        foreach (var id in itemsChanged)
        {
            var item = itemDatabase.GetItem(id.Key);
            var wallet = Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet;
            var amount = wallet.ItemAmount(id.Key);
            if (item is Buff buff && amount > 0 && !activeBuffs.Contains(buff))
            {
                activeBuffs.Add(buff);
            }
        }
    }
}