using System;
using System.Collections.Generic;
public class TimeRewardManager
{
    public int currentRewardIndex;
    public long finishTime;
    public bool canCollect;
    public bool loaded;

    public TimeRewardManager()
    {
        currentRewardIndex = 0;
        canCollect = false;
        loaded = false;
    }

    public void ClaimItems(Dictionary<Guid, long> rewards)
    {
        currentRewardIndex++;
        loaded = false;
        canCollect = false;
        var wallet = Services.Container.Resolve<Wallet>();
        
        wallet.GiveItems(rewards);
        OverlayCanvas.Instance.ShowDrops(rewards);
        Services.Container.Resolve<InventoryService>().Save();
    }
}
