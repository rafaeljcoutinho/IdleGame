using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fishing
{
    public class FishingSpot : BaseInteractableNodeMonobehaviour
    {
        [SerializeField] private ActionNodeData actionNodeData;
        [SerializeField] private EnemyHpController hpController;
        [SerializeField] private Transform dropAnchor;
        
        public override NodeData NodeData => actionNodeData;
        public override Type PlayerBehaviour => typeof(FishingBehaviour);

        private void Awake()
        {
            hpController.SetMaxHp(actionNodeData.hp);
            hpController.ResetHp();
            hpController.OnDeath += OnDeath;
        }

        private void OnDeath(SkillService.OverkillInfo overkillInfo)
        {
            Services.Container.Resolve<OverkillService>().NotifyEnemyDied(NodeData);
            DropFromDroptable(SkillData.Type.Fishing, hpController, actionNodeData, dropAnchor);
            hpController.ResetHp();
        }

        public override void Highlight()
        {
            base.Highlight();
            var chs = Services.Container.Resolve<ContextualHintService>();
            chs.SetDisplayOptions(new ContextualHintService.HintDisplayOptions
                {
                    offset = .5f,
                    anchorPosition = Contextual2DObject.AnchorPosition.Auto,
                    fadeBackground = false,
                    dismissOnScreenInteraction = false,
                })
                .SetTargetTransform(transform)
                .Show(new TextLayout.LayoutData {
                    LocalizedText = Services.Container.Resolve<LocalizationService>().LocalizeText("Fishing spot")
                });
        }

        public override void DeHighlight()
        {
            base.DeHighlight();
            Services.Container.Resolve<ContextualHintService>().Hide(false);
        }

        public void TakeDamage(int progress)
        {
            hpController.TakeDamage(new HitInfo
            {
                Damage = progress
            });
        }
    }
}