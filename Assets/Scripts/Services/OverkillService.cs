using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OverkillService
{
    public event Action<NodeData, int> OnNodeDeath;

    private Dictionary<Guid, OverkillRecord> OverkillRecords = new (10);
    private const float OverkillBuffExpire = 7f;
    
    public void NotifyEnemyDied(NodeData node, int count = 1, SkillService.OverkillInfo overkillInfo = null)
    {
        if (overkillInfo == null)
        {
            overkillInfo = SkillService.OverkillInfo.NoOverkill;
        }
        if (!OverkillRecords.ContainsKey(node.Uuid))
        {
            OverkillRecords.Add(node.Uuid, new OverkillRecord
            {
                expireTime = Time.time + OverkillBuffExpire,
                overkillInfo = overkillInfo,
            });
        }
        else
        {
            var currentOverkill = OverkillRecords[node.Uuid];
            if (overkillInfo.IsOverkill && currentOverkill.overkillInfo.OverkillAmount < overkillInfo.OverkillAmount)
            {
                currentOverkill.overkillInfo = overkillInfo;
                currentOverkill.expireTime = Time.time + OverkillBuffExpire;
            }
        }
        
        OnNodeDeath?.Invoke(node, count);
    }

    public Dictionary<Guid, OverkillRecord> GetAllOverkillRecords()
    {
        foreach (var id in OverkillRecords.Keys.ToList())
        {
            var isExpired = Time.time > OverkillRecords[id].expireTime;
            if (isExpired)
            {
                OverkillRecords.Remove(id);
            }
        }

        return OverkillRecords;
    }
    
    public OverkillRecord GetOverkillRecord(Guid uuid)
    {
        if (OverkillRecords.ContainsKey(uuid))
        {
            var isExpired = Time.time > OverkillRecords[uuid].expireTime;
            if (isExpired)
            {
                OverkillRecords.Remove(uuid);
                return null;
            }
            return OverkillRecords[uuid];
        }

        return null;
    }

    public class OverkillRecord
    {
        public float expireTime;
        public SkillService.OverkillInfo overkillInfo;
    }
}
