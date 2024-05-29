using System;
using System.Collections.Generic;
using UnityEngine;
using Woodcutting;

namespace InteractableNodes
{
    public class MapChangeNode : BaseInteractableNodeMonobehaviour
    {
        [SerializeField] MapNodeConfig targetNode;
        [SerializeField] private GameplaySceneBootstrapper gameplaySceneBootstrapper;
        [SerializeField] private MapKillcountIndicator indicatorPrefab;
        [SerializeField] private Vector3 indicatorOffset;
        [SerializeField] private Color portalEnabled;
        [SerializeField] private Color portalLocked;
        [SerializeField] private List<ParticleSystem> portalEffects;
        [SerializeField] private Requirement requirementToShow;

        public override Type PlayerBehaviour => typeof(CloseDistanceBehaviour);
        public override NodeData NodeData => null;

        private MapKillcountIndicator indicatorInstance;
        private MapProgression MapProgression => Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression;

        private const string PORTAL_LOCKED_TEXT = "Portal is locked. Kill enemies to unlock";
        private bool CanShow => Services.Container.Resolve<InventoryService>().PlayerProfile.MeetsRequirement(requirementToShow).HasRequirements; 

        private void OnEnable()
        {
            if (!CanShow)
            {
                gameObject.SetActive(false);
                Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer.OnQuestCompleted += QuestCompleted;
                return;
            }
            
            if (MapProgression.unlockedMaps.Contains(targetNode.Uuid)) 
                return;
            indicatorInstance = OverlayCanvas.Instance.DynamicUIIndicatorContainer.InstantiateUI(indicatorPrefab);
            UpdateIndicator();
            indicatorInstance.Show(transform, indicatorOffset);
            Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.MapUnlockMadeProgress += MapUnlockMadeProgress;
            Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.NewMapUnlocked += NewMapUnlocked;
        }

        private void QuestCompleted(Guid questId)
        {
            if (requirementToShow.HasQuestRequirement(questId))
            {
                if (CanShow)
                {
                    Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer.OnQuestCompleted -= QuestCompleted;
                    gameObject.SetActive(true);
                }
            }
        }

        private void NewMapUnlocked(Guid guid)
        {
            if (targetNode.Id != guid) 
                return;
            UpdateIndicator();
            Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.MapUnlockMadeProgress -= MapUnlockMadeProgress;
            Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.NewMapUnlocked -= MapUnlockMadeProgress;
        }
        
        private void MapUnlockMadeProgress(Guid obj)
        {
            if (obj != targetNode.Id) 
                return;
            UpdateIndicator();
        }

        private const string kcFormatted = "<size=75%>x</size>{0}";
        private void UpdateIndicator()
        {
            if (indicatorInstance == null)
            {
                return;
            }
            if (MapProgression.unlockedMaps.Contains(targetNode.Uuid))
            {
                foreach (var particle in portalEffects)
                {
                    var psMain = particle.main;
                    psMain.startColor = portalEnabled;
                }
                indicatorInstance.Hide();
                indicatorInstance = null;
                return;
            }
            foreach (var particle in portalEffects)
            {
                var psMain = particle.main;
                psMain.startColor = portalLocked;
            }
            var icon = targetNode.Objective.targetNode.icon;
            var text = string.Format(kcFormatted, MapProgression.KillsLeft(targetNode.Id));
            indicatorInstance.SetData(icon, text);
        }

        private void OnDisable()
        {
            if (indicatorInstance != null)
            {
                indicatorInstance.Hide();
                indicatorInstance = null;
            }
            Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.MapUnlockMadeProgress -= MapUnlockMadeProgress;
            Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.NewMapUnlocked -= MapUnlockMadeProgress;
        }

        private void OnDestroy()
        {
            Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer.OnQuestCompleted -= QuestCompleted;
        }

        public void ChangeMap(MapNodeConfig mapNodeConfig)
        {
            Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.TryUnlockMap(mapNodeConfig.Id);
            var unlocked = Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.unlockedMaps.Contains(mapNodeConfig.Id);
            if (!unlocked)
            {
                return;
            }
            OverlayCanvas.Instance.ScreenEffects.FadeOut(() =>
            {
                var ok = Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.Transition(mapNodeConfig);
                if (!ok)
                {
                    OverlayCanvas.Instance.ScreenEffects.FadeIn();
                }
            });
        }
        
        public override void Interact()
        {
            var unlocked = Services.Container.Resolve<InventoryService>().PlayerProfile.MapProgression.unlockedMaps.Contains(targetNode.Id);
            if (!unlocked)
            {
                Services.Container.Resolve<ContextualHintService>().SetDisplayOptions(new ContextualHintService.HintDisplayOptions
                    {
                        offset = .5f,
                        autoHideSeconds = 3,
                        anchorPosition = Contextual2DObject.AnchorPosition.Top,
                    }).SetTargetTransform(transform)
                    .Show(new TextLayout.LayoutData
                    {
                        LocalizedText = PORTAL_LOCKED_TEXT,
                    });
                return;
            }
            
            
            gameplaySceneBootstrapper.Player.playerBehaviourFSM.GetState<CloseDistanceBehaviour>().SetTarget(transform, .6f, typeof(DoNothingBehaviour),
                () =>
                {
                    ChangeMap(targetNode);
                });
        }
    }
}