using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorBootstrap : MonoBehaviour
{
    [SerializeField] private string nextScene = "Map-1";
    [SerializeField] private OverlayCanvas overlayCanvas;
    [SerializeField] private ScreenEffects screenEffects;

    private Services services;

    private bool profileLoaded;
    private bool isNewPlayer;
    private PlayerProfile profile;

    IEnumerator Start()
    {
        if (Application.isMobilePlatform)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            var loadingScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            SceneManager.UnloadSceneAsync(loadingScene);
        };

        services = new Services();
        services.OnPlayerProfileLoad += ServicesOnOnPlayerProfileLoad;
        services.Initialize((ok) =>
        {
            if (ok)
            {
                Services.Container.Resolve<SkillService>().InitializeLevelUpBroker();
                Services.Container.Resolve<InventoryService>().PlayerProfile.OnAfterLoad();

                Services.Container.bindings.Add(typeof(OverlayCanvas), _ =>
                {
                    var canvasInstance = Instantiate(overlayCanvas);
                    DontDestroyOnLoad(canvasInstance);
                    return canvasInstance;
                });
            }
        });

        while (!profileLoaded)
            yield return null;

        var scene = nextScene;
        if (!isNewPlayer && profile?.OfflineTracker?.OfflineActivityRecord?.scene != null)
        {
            scene = profile.OfflineTracker.OfflineActivityRecord.scene;
        }

        var sceneLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        if (sceneLoad == null)
        {
            sceneLoad = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        }

        sceneLoad.allowSceneActivation = false;

        while (sceneLoad.progress < .9f || !services.Initialized)
        {
            yield return null;
        }

        Services.Container.Resolve<OverlayCanvas>().ScreenEffects.FadeOut();
        screenEffects.FadeOut(() =>
        {
            sceneLoad.allowSceneActivation = true;
        });
    }

    private void ServicesOnOnPlayerProfileLoad(PlayerProfile profile, bool isNewPlayer)
    {
        this.profile = profile;
        this.isNewPlayer = isNewPlayer;
        profileLoaded = true;
    }
}