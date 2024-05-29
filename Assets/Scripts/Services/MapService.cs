using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[Serializable]
public class MapProgression
{
    public HashSet<Guid> unlockedMaps;
    public Dictionary<Guid, QuestProgressContainer.QuestStepRequirementProgress> inProgressMaps;

    [NonSerialized] public Action<Guid> NewMapUnlocked;
    [NonSerialized] public Action<Guid> MapUnlockMadeProgress;

    public void Init()
    {
        if (unlockedMaps == null)
        {
            unlockedMaps = new HashSet<Guid>();
        }

        if (inProgressMaps == null)
        {
            inProgressMaps = new Dictionary<Guid, QuestProgressContainer.QuestStepRequirementProgress>();
        }

        if (!unlockedMaps.Contains(Guid.Parse("2fa32ae2-39fc-43d1-9920-bcf8bc4c0ba0")))
        {
            TryUnlockMap(Guid.Parse("2fa32ae2-39fc-43d1-9920-bcf8bc4c0ba0"), true);
        }
        Services.Container.Resolve<OverkillService>().OnNodeDeath += OnNodeDeath;
        Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer.OnQuestCompleted += QuestCompleted;
        Services.Container.Resolve<SkillService>().OnSkillLevelsChanged += OnSkillLevelsChanged;
    }

    private void OnSkillLevelsChanged(List<SkillService.LevelupEvent> levelsChanged)
    {
        foreach (var mapId in inProgressMaps.Keys.ToList())
        {
            var config = GetConfig(mapId);
            foreach (var questData in config.Requirement.SkillLevels)
            {
                foreach (var levelUp in levelsChanged)
                {
                    if (levelUp.skill == questData.skill)
                    {
                        TryUnlockMap(mapId);
                        return;
                    }
                }
            }
        }
    }

    public bool Transition(MapNodeConfig map)
    {
        if (unlockedMaps.Contains(map.Id))
        {
            Services.Container.Resolve<CoroutineDispatcher>().Run(MoveToMap(map.SceneName));
            return true;
        }

        return false;
    }

    private void QuestCompleted(Guid questId)
    {
        foreach (var mapId in inProgressMaps.Keys.ToList())
        {
            var config = GetConfig(mapId);
            foreach (var questData in config.Requirement.CompletedQuests)
            {
                if (questId == questData.Uuid)
                {
                    TryUnlockMap(mapId);
                    return;
                }
            }
        }
    }

    public IEnumerator MoveToMap(string scene)
    {
        var sceneLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        sceneLoad.allowSceneActivation = false;

        while (sceneLoad.progress < .9f)
        {
            yield return null;
        }
        GC.Collect();
        sceneLoad.allowSceneActivation = true;
    }

    private void OnNodeDeath(NodeData obj, int kc)
    {
        foreach (var mapId in inProgressMaps.Keys.ToList())
        {
            if (inProgressMaps[mapId].id == obj.Uuid)
            {
                inProgressMaps[mapId].amount += kc;
                MapUnlockMadeProgress?.Invoke(mapId);
                TryUnlockMap(mapId);
            }
        }
    }

    private MapNodeConfig GetConfig(Guid id)
    {
        return Services.Container.Resolve<NodeDatabaseService>().NodeDatabase.GetNode(id) as MapNodeConfig;
    }

    public bool CanUnlockMap(Guid id)
    {
        if (unlockedMaps.Contains(id))
        {
            return false;
        }
        var config = GetConfig(id);
        var playerProfile = Services.Container.Resolve<InventoryService>().PlayerProfile;
        var meetsRequirement = playerProfile.MeetsRequirement(config.Requirement);
        if (!meetsRequirement.HasRequirements)
        {
            return false;
        }

        if (!inProgressMaps.ContainsKey(id))
        {
            return false;
        }
        var myProgress = inProgressMaps[id].amount;
        var target = config.Objective.amount;
        if (myProgress < target)
        {
            return false;
        }

        return true;
    }

    public int KillsLeft(Guid id)
    {
        if (!inProgressMaps.ContainsKey(id))
        {
            return -1;
        }

        var config = GetConfig(id);
        var targetKc = config.Objective.amount;
        var currentKc = inProgressMaps[id].amount;
        return targetKc - currentKc;
    }
    
    public bool TryUnlockMap(Guid id, bool bypassChecks = false)
    {
        if (!bypassChecks && !CanUnlockMap(id))
        {
            return false;
        }
        unlockedMaps.Add(id);
        NewMapUnlocked?.Invoke(id);
        inProgressMaps.Remove(id);

        var config = GetConfig(id);
        if (config.NextMap != null)
        {
            inProgressMaps.Add(config.NextMap.Uuid, new QuestProgressContainer.QuestStepRequirementProgress
            {
                id = config.NextMap.Objective.targetNode.Uuid,
                amount = 0,
            });
        }
            
        Services.Container.Resolve<InventoryService>().Save();
        return true;
    }
}