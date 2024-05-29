using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyAreaData enemyAreaData;
    [SerializeField] private GameplaySceneBootstrapper bootstrapper;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private Droptable chest;
    
    public event Action OnAreaReset;
    public event Action OnKillstreakTierup;
    public event Action OnKillstreakProgress;
    public event Action OnTimerRunOut;

    private Coroutine areaResetRoutine;

    private int currentKillCount = 0;
    public int TargetKillCount => 10;
    public int CurrentKillCountKc => currentKillCount;
    public EnemyAreaData AreaData => enemyAreaData;
    private List<SpawnPoint> spawnPointInfos = new();
    private List<float> nextSpawns;
    public List<Enemy> Enemies => spawnPointInfos.Select(t => t.spawnedEnemy).Where(t => t != null).ToList();
    public bool CanOpenChest => currentKillCount >= TargetKillCount;
    public Player player => bootstrapper.Player;

    public bool IsInBounds(Player player)
    {
        return Vector3.Distance(player.transform.position.HorizontalPlane(), transform.position.HorizontalPlane()) < 5f;
    }

    void Start()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            spawnPointInfos.Add(new SpawnPoint
            {
                point = spawnPoint,
                nextSpawnTime = 0,
                hasEnemyToSpawn = false,
            });
        }
        bootstrapper.Player.HpController.OnDeath += OnPlayerDeath;
        StartSpawning();
    }

    void StartSpawning(float delay = 0)
    {
        StartCoroutine(StartSpawnEnemiesRoutine(enemyAreaData, delay));
    }
    
    public void OnPlayerDeath(SkillService.OverkillInfo _)
    {
        currentKillCount = 0;
        Services.Container.Resolve<InventoryService>().PlayerProfile.SlayerManager.PlayerDeath();
        Services.Container.Resolve<InventoryService>().Save();
        OnAreaReset?.Invoke();
        Services.Container.Resolve<CoroutineDispatcher>().AfterDelay(4f, () =>
        {
            bootstrapper.Player.HpController.Respawn(.15f);
        });
    }

    IEnumerator StartSpawnEnemiesRoutine(EnemyAreaData waveData, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        var awaiter = new WaitForSeconds(.1f);
        InstantSpawn(waveData.enemy, waveData.quantity);
        while (true)
        {
            var aliveEnemies = spawnPointInfos.Count(t =>  t.CountsAsAlive());
            if (aliveEnemies < waveData.quantity)
            {
                InstantSpawn(waveData.enemy, waveData.quantity - aliveEnemies);
            }
            
            ProcessScheduledSpawns(waveData.enemy);
            yield return awaiter;
        }
    }

    void ProcessScheduledSpawns(ActionNodeData enemyData)
    {
        foreach (var sp in spawnPointInfos)
        {
            if (sp.hasEnemyToSpawn && Time.time >= sp.nextSpawnTime)
            {
                var selectedSpawnPoint = sp;
                var enemy = Spawn(enemyData, selectedSpawnPoint.point);
                enemy.OnAfterEnemyDeath += (overkillInfo) =>
                {
                    OnEnemyDeath(enemy, selectedSpawnPoint,overkillInfo);
                };
                selectedSpawnPoint.spawnedEnemy = enemy;
                selectedSpawnPoint.hasEnemyToSpawn = false;
            }
        }
    }
    
    void InstantSpawn(ActionNodeData enemyData, int quantity)
    {
        var spawnPointsRandom = spawnPointInfos.Where(t => t.CanSpawn()).OrderBy(x => Guid.NewGuid()).ToList();
        for (var i = 0; i < quantity && i < spawnPointsRandom.Count; i++)
        {
            var selectedSpawnPoint = spawnPointsRandom[i];
            var enemy = Spawn(enemyData, selectedSpawnPoint.point);
            enemy.OnAfterEnemyDeath += (overkillInfo) =>
            {
                OnEnemyDeath(enemy, selectedSpawnPoint, overkillInfo);
            };
            selectedSpawnPoint.spawnedEnemy = enemy;
        }
    }

    private Enemy Spawn(ActionNodeData selectedEnemy, Transform root)
    {
        var enemy = Instantiate(selectedEnemy.prefab, root.position, root.rotation, enemyContainer).GetComponent<Enemy>();
        enemy.SetEnemySpawner(this);
        var level = enemyAreaData.enemy.level;
        enemy.Initialize(bootstrapper.Player, level, bootstrapper.OutlineManager);
        return enemy;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position.HorizontalPlane(), 5f);
        foreach (var sp in spawnPointInfos)
        {
            Gizmos.color = Color.green;
            if (sp.spawnedEnemy != null && sp.spawnedEnemy.IsAlive)
                Gizmos.DrawSphere(sp.point.position, .2f);
            
            Gizmos.color = Color.white;
            if (sp.hasEnemyToSpawn)
            {
                var timeLeft = sp.nextSpawnTime - Time.time;
                Gizmos.DrawSphere(sp.point.position, timeLeft/50);
            }
        }
    }
    
    private void OnEnemyDeath(Enemy enemy, SpawnPoint spawnPoint, SkillService.OverkillInfo overkillInfo)
    {
        var canGetChest = Services.Container.Resolve<InventoryService>().PlayerProfile.SlayerManager.CanGetMoreChests();
        if (canGetChest)
        {
            currentKillCount++;
            OnKillstreakProgress?.Invoke();
            if (currentKillCount >= TargetKillCount)
            {
                TryGiveChest();
            }
        }
        
        spawnPoint.hasEnemyToSpawn = false;
        spawnPoint.spawnedEnemy = null;

        var spawnPointsRandom = spawnPointInfos.Where(t => t.CanSpawn()).ToList();
        var randomIndex = Random.Range(0, spawnPointsRandom.Count);
        spawnPointsRandom[randomIndex].nextSpawnTime = Time.time + enemyAreaData.enemy.respawnCooldown;
        spawnPointsRandom[randomIndex].hasEnemyToSpawn = true;
    }

    private bool TryGiveChest()
    {
        if (currentKillCount < TargetKillCount)
        {
            return false;
        }

        currentKillCount = 0;
        Services.Container.Resolve<InventoryService>().PlayerProfile.SlayerManager.GiveChest(Guid.Parse(chest.id));
        Services.Container.Resolve<InventoryService>().Save();
        OnKillstreakTierup?.Invoke();
        return true;
    }
}
