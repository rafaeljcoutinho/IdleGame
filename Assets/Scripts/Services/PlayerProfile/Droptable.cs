using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Droptable", menuName = "Droptable/Droptable")]
public class Droptable : ScriptableObject
{
    [ScriptableObjectId] public string id;
    [SerializeField] private List<Drop> randomDrops;
    [SerializeField] private List<Drop> guaranteedDrops;
    
    public List<Drop> RandomDrops => randomDrops;
    public List<Drop> GuaranteedDrops => guaranteedDrops;

    private long totalWeight;

    [Serializable]
    public class Drop
    {
        public Droptable droptable;
        public Item item;
        public AnimationCurve distribution;
        public long minAmount;
        public long maxAmount;
        public float percentage;
    }
}