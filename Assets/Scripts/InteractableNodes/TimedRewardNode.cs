using System;
using DG.Tweening;
using UnityEngine;
using Woodcutting;

public class TimedRewardNode : BaseInteractableNodeMonobehaviour
{
    [SerializeField] private Droptable droptable;
    [ScriptableObjectId] [SerializeField] private string myGuidToken;
    [SerializeField] private long cooldownInSeconds;
    [SerializeField] private ParticleSystem ctaParticles;
    [SerializeField] private GameplaySceneBootstrapper gameplaySceneBootstrapper;

    public override NodeData NodeData => null;
    public override Type PlayerBehaviour => typeof(CloseDistanceBehaviour);
    private Guid TimerLockId => Guid.Parse(myGuidToken);

    [SerializeField] private Transform animatedOpening;
    private bool canOpen = false;

    private TextLayout.LayoutData hintLayoutData;
    private float hintDismissTimer;


    void Start()
    {
        var timeLock = Services.Container.Resolve<InventoryService>().PlayerProfile.TransientWallet
            .ItemAmount(TimerLockId);
        canOpen = OfflineTracker.TimeNowUnix > timeLock;
        if (canOpen)
        {
            animatedOpening.localRotation = Quaternion.identity;
            ctaParticles.Play();
        }
        else
        {
            var finalRotation = Quaternion.Euler(-75, 0, 0);
            animatedOpening.localRotation = finalRotation;
            ctaParticles.Stop();
        }
    }

    void AnimateOpening(Action onComplete)
    {
        var finalRotation = Quaternion.Euler(-75, 0, 0);
        transform.DOPunchRotation(Vector3.one * 5, .5f);
        animatedOpening.DOLocalRotateQuaternion(finalRotation, .7f).SetEase(Ease.OutExpo)
            .OnComplete(() => onComplete?.Invoke());
        transform.DOPunchPosition(Vector3.up * .3f, .3f);
        ctaParticles.Stop();
    }

    private void Update()
    {
        // skip a few frames to update
        if (Time.frameCount % 5 != 0)
            return;

        var timeLock = Services.Container.Resolve<InventoryService>().PlayerProfile.TransientWallet.ItemAmount(TimerLockId);
        if (hintLayoutData != null && Time.time < hintDismissTimer)
        {
            var waitTimeSeconds = timeLock - OfflineTracker.TimeNowUnix;
            var time = TimeSpan.FromSeconds(waitTimeSeconds + 1);
            var duration = 3f;
            hintLayoutData.LocalizedText = canOpen ? "Ready to open" : $"Open again in {time:hh\\:mm\\:ss}";
        }
        
        if (!canOpen && OfflineTracker.TimeNowUnix > timeLock)
        {
            animatedOpening.DOLocalRotateQuaternion(Quaternion.identity, .5f).SetEase(Ease.OutExpo);
            transform.DOPunchRotation(Vector3.one, .5f);
            ctaParticles.Play();
        }

        canOpen = OfflineTracker.TimeNowUnix > timeLock;
    }

    public override void Interact()
    {
        base.Interact();
        gameplaySceneBootstrapper.Player.playerBehaviourFSM.GetState<CloseDistanceBehaviour>().SetTarget(transform, .6f,
            typeof(DoNothingBehaviour),
            () =>
            {
                var timeLock = Services.Container.Resolve<InventoryService>().PlayerProfile.TransientWallet
                    .ItemAmount(TimerLockId);
                var canOpen = OfflineTracker.TimeNowUnix > timeLock;
                if (canOpen)
                {
                    Services.Container.Resolve<InventoryService>().PlayerProfile.TransientWallet
                        .SetItem(TimerLockId, OfflineTracker.TimeNowUnix + cooldownInSeconds);
                    AnimateOpening(() =>
                    {
                        var reward = Services.Container.Resolve<DropGeneratorService>().GenerateDrops(droptable, 1);
                        Services.Container.Resolve<Wallet>().AddPendingItems(TimerLockId, reward);

                        var itemDatabase = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;
                        var time = TimeSpan.FromSeconds(cooldownInSeconds);
                        OverlayCanvas.Instance.AfkProgressPopup._ViewData.drops = itemDatabase.ToItemDict(reward);
                        OverlayCanvas.Instance.AfkProgressPopup._ViewData.title = "CHEST REWARDS";
                        OverlayCanvas.Instance.AfkProgressPopup._ViewData.subtitle =
                            $"open again in {time:hh\\:mm\\:ss}";
                        OverlayCanvas.Instance.AfkProgressPopup._ViewData.showVideoButton = false;
                        OverlayCanvas.Instance.AfkProgressPopup._ViewData.leftImage = null;
                        OverlayCanvas.Instance.AfkProgressPopup.Show(result =>
                        {
                            var rewards = Services.Container.Resolve<Wallet>().ApplyPendingItems(TimerLockId);
                            Services.Container.Resolve<InventoryService>().Save();
                            OverlayCanvas.Instance.ShowDrops(rewards);
                        });
                        Services.Container.Resolve<InventoryService>().Save();
                    });
                }
                else
                {
                    var waitTimeSeconds = timeLock - OfflineTracker.TimeNowUnix;
                    var time = TimeSpan.FromSeconds(waitTimeSeconds + 1);
                    var duration = 3f;
                    hintLayoutData = new TextLayout.LayoutData
                    {
                        LocalizedText = $"Open again in {time:hh\\:mm\\:ss}",
                    };
                    Services.Container.Resolve<ContextualHintService>().SetDisplayOptions(new ContextualHintService.HintDisplayOptions
                    {
                        anchorPosition = Contextual2DObject.AnchorPosition.Top,
                        autoHideSeconds = duration,
                        fadeBackground = false,
                        offset = 2f,
                    }).SetTargetTransform(transform).Show(hintLayoutData);
                    hintDismissTimer = Time.time + duration;
                }
            });
    }
}