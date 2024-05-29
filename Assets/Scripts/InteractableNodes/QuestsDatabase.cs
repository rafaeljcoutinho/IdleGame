using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Database")]
public class QuestsDatabase : ScriptableObject
{
    [SerializeField] private List<QuestData> Quests;
    private Dictionary<Guid, QuestData> questDict;
    public List<QuestData> AllQuests => Quests;

    public void Start()
    {
        questDict = new Dictionary<Guid, QuestData>();
        foreach (var item in Quests)
        {
            questDict.Add(Guid.Parse(item.id), item);
        }
    }

    public QuestData GetQuest(Guid guid)
    {
        if (!questDict.ContainsKey(guid))
            return null;
        return questDict[guid];
    }
}