using System.Collections.Generic;
using UnityEngine;

public class GameplaySceneBootstrapper : MonoBehaviour
{
    [SerializeField] private Transform playerDefaultSpawnPosition;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private Joystick joystick;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private List<EnemyManager> enemySpawner;
    [SerializeField] private OutlineManager outlineManager;

    private Player player;
    public Player Player => player;
    public Joystick Joystick => joystick;
    public List<EnemyManager> EnemySpawner => enemySpawner;
    public OutlineManager OutlineManager => outlineManager;
    

    public Camera Camera => mainCamera;
    
    private void Awake()
    {
        var worldPosition = Services.Container.Resolve<InventoryService>().PlayerProfile.OfflineTracker?.OfflineActivityRecord?.worldPosition;
        player = Instantiate(playerPrefab, playerDefaultSpawnPosition.position, Quaternion.Euler(new Vector3(0,180,0)));   
        player.Initialize(this, false ? new Vector3(worldPosition[0], worldPosition[1], worldPosition[2]) : playerDefaultSpawnPosition.position);
        Services.Container.Resolve<OverlayCanvas>().OnSceneChange(Camera, this);
        Services.Container.Resolve<OverlayCanvas>().ScreenEffects.FadeIn();
    }

    private void LateUpdate()
    {
        Services.Container.Resolve<InventoryService>().OnLateUpdate();
    }
}
