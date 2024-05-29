using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class OfflineTracker
{
    public static long TimeNowUnix => (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
    
    public OfflineActivityRecord OfflineActivityRecord;
    [NonSerialized] private const float AutoSaveTime = 10f;
    [NonSerialized] private Guid lastSavedNode;
    [NonSerialized] private float lastSaveTime = 0;
    
    public void RegisterActivity(IInteractableNode node, Player player)
    {
        OfflineActivityRecord ??= new OfflineActivityRecord
        {
            worldPosition = new[] { 0f, 0f, 0f }
        };
         OfflineActivityRecord.startTime = TimeNowUnix;
         OfflineActivityRecord.scene = SceneManager.GetActiveScene().name;
         OfflineActivityRecord.worldPosition = new[] { player.transform.position.x, player.transform.position.y, player.transform.position.z };

         var hasNodeData = node != null && node.NodeData != null;
         OfflineActivityRecord.node = hasNodeData ? node.NodeData.Uuid : OfflineActivityRecord.node;
         OfflineActivityRecord.skill = hasNodeData ? player.GetBehaviour(node.PlayerBehaviour).SkillUsed : OfflineActivityRecord.skill;

         var shouldSave = lastSavedNode != OfflineActivityRecord.node || Time.time > lastSaveTime + AutoSaveTime;
         if (shouldSave)
         {
             Services.Container.Resolve<InventoryService>().Save();
             lastSaveTime = Time.time;
             lastSavedNode = OfflineActivityRecord.node;
         }
    }

    long GetMaxAfkTime()
    {
        var isPremiumPlayer = false;
        if (isPremiumPlayer)
        {
            // one week
            return 3600 * 24 * 7;
        }

        return 20 * 3600;
    }

    public class OfflineProgressResult
    {
        public Dictionary<Guid, long> Items;
        public long TimeInSeconds;
        public int kills;
        public Sprite nodeIcon;
    }
    
    public OfflineProgressResult GetOfflineProgression()
    {
        if (OfflineActivityRecord == null || OfflineActivityRecord.node == Guid.Empty || OfflineActivityRecord.skill == null)
            return null;

        var node = OfflineActivityRecord.node;
        var nodeData = Services.Container.Resolve<NodeDatabaseService>().NodeDatabase.GetNode(node);
        if (nodeData == null)
            return null;
        
        var timeInSeconds = TimeNowUnix-OfflineActivityRecord.startTime;
        timeInSeconds = Math.Min(timeInSeconds, GetMaxAfkTime());

        var progress = Services.Container.Resolve<SkillService>()
            .GetAfkProgress(OfflineActivityRecord.skill.Value, nodeData as ActionNodeData, Convert.ToInt32(timeInSeconds));

        return new OfflineProgressResult
        {
            Items = progress.Item2,
            kills = progress.Item1,
            TimeInSeconds = timeInSeconds,
            nodeIcon = nodeData.icon,
        };
    }

    public void ResetOfflineActivity()
    {
        if (OfflineActivityRecord != null)
        {
            OfflineActivityRecord.startTime = TimeNowUnix;
        }
    }
}

[Serializable]
public class OfflineActivityRecord
{
    public long startTime;
    public float[] worldPosition;
    public string scene;
    public Guid node;
    public SkillData.Type? skill;
}