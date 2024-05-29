using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class WaveData
{
    public EnemyData enemyData;
    public int killsToTierUp; // -1 means infinite
    public float respawn;
    public int quantity;
}

[Serializable]
public class EnemyData
{
    public GameObject prefab;
    public int level;
}