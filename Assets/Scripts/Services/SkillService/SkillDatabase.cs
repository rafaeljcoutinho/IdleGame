using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDatabase", menuName = "Skill/Database")]
public class SkillDatabase : ScriptableObject
{
    [SerializeField] private List<SkillData> Skills;
    private Dictionary<SkillData.Type, SkillData> skillDictionary;

    public void Start()
    {
        skillDictionary = new Dictionary<SkillData.Type, SkillData>();
        foreach (var item in Skills)
        {
            skillDictionary.Add(item.SkillType, item);
        }
    }

    public SkillData GetSkillData(SkillData.Type type)
    {
        if (!skillDictionary.ContainsKey(type))
            return null;
        return skillDictionary[type];
    }
}