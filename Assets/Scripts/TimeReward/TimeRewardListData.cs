using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="TimeRewardListData", menuName = "TimeReward/TimeRewardListData")]
public class TimeRewardListData : ScriptableObject
{
    public List<TimeRewardData> timeRewardDatas;

    private void OnValidate() {
        List<string> ids = new();
        foreach (var item in timeRewardDatas)
        {
            if (ids.Contains(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }
            ids.Add(item.Id);
        }
    }

}