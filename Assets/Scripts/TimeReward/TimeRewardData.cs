using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TimeRewardData
{
    [ScriptableObjectId] public string Id;
    public Sprite Icon;
    public List<ItemWithQuantity> Rewards;

    public int Days;
    [Range(0, 23)]
    public int Hours;
    [Range(0, 59)]
    public int Minutes;

}
