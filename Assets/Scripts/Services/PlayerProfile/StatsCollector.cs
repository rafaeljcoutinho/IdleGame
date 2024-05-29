using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatsCollector
{
    public KillCountStats KillCountStats;
    
    public void Init()
    {
        var overkillService = Services.Container.Resolve<OverkillService>();
        overkillService.OnNodeDeath += OverkillServiceOnOnNodeDeath;

        if (KillCountStats == null)
        {
            KillCountStats = new KillCountStats();
        }
    }

    private void OverkillServiceOnOnNodeDeath(NodeData nodeData, int kc)
    {
        var overkillRecord = Services.Container.Resolve<OverkillService>().GetOverkillRecord(nodeData.Uuid);
        if (!KillCountStats.KillsByEnemy.ContainsKey(nodeData.Uuid))
        {
            KillCountStats.KillsByEnemy.Add(nodeData.Uuid, kc);
        }
        else
        {
            KillCountStats.KillsByEnemy[nodeData.Uuid] += kc;
        }
        
    }
}

[Serializable]
public class KillCountStats
{
    public Dictionary<Guid, long> KillsByEnemy;

    public KillCountStats()
    {
        KillsByEnemy = new Dictionary<Guid, long>();
    }

    public long GetKC(Guid nodeDataId)
    {
        return KillsByEnemy.ContainsKey(nodeDataId) ? KillsByEnemy[nodeDataId] : 0;
    }
}