using UnityEngine;

public class SpawnPoint
{
    public Transform point;
    public float nextSpawnTime;
    public Enemy spawnedEnemy;
    public bool hasEnemyToSpawn;

    public bool CanSpawn()
    {
        return (spawnedEnemy == null || !spawnedEnemy.IsAlive) && !hasEnemyToSpawn;
    }

    public bool CountsAsAlive()
    {
        return spawnedEnemy?.IsAlive ?? false || hasEnemyToSpawn;
    }
}