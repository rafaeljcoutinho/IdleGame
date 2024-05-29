using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mining
{
    public class MiningSkillNode : BaseInteractableNodeMonobehaviour
    {
        [SerializeField] private ActionNodeData actionNodeData;
        [SerializeField] private EnemyHpController hpController;
        
        public override NodeData NodeData => actionNodeData;
        public override Type PlayerBehaviour => typeof(MiningBehaviour);

        private void Awake()
        {
            hpController.ResetHp();
            hpController.OnDeath += OnDeath;
        }

        private void OnDeath(SkillService.OverkillInfo _)
        {
            var itemDrops = Services.Container.Resolve<DropGeneratorService>().GenerateDrops(actionNodeData.droptable, 1); 
            var inventory = Services.Container.Resolve<InventoryService>();
            inventory.PlayerProfile.Wallet.GiveItems(itemDrops);
            OverlayCanvas.Instance.ShowDrops(itemDrops);
            inventory.Save();
            hpController.ResetHp();
        }

        public override void Highlight()
        {
            outline.enabled = true;
        }

        public override void DeHighlight()
        {
            outline.enabled = false;
        }

        public void TakeDamage(int damage)
        {
            hpController.TakeDamage(new HitInfo
            {
                Damage = damage,
            });
        }
    }
}