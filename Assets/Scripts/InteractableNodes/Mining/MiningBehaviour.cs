using System;
using UnityEngine;
using Woodcutting;

namespace Mining
{
    public class MiningBehaviour : Player.Behaviour
    {
        public const float minDistance = .4f;
        private float actionCooldown = 0f;
        private float actionTimer = 0f;
        private const string actionAnimationName = "mining";

        private Player player;
        private MiningSkillNode skillNode;
        public override SkillData.Type SkillUsed => SkillData.Type.Mining;
        private SkillData.Type skill = SkillData.Type.Mining;
        private Equipment.EquipSlot toolSlot = Equipment.EquipSlot.ToolBeltPickaxe;

        public MiningBehaviour(Player player)
        {
            this.player = player;
        }

        public override Type Update(float dt)
        {
            if (skillNode == null)
            {
                player.Animator.animator.SetBool(actionAnimationName, false);
                return GetType();
            }

            if (NeedsToCloseInDistance(skillNode.transform))
            {
                player.Animator.animator.SetBool(actionAnimationName, false);
                var closeDistance = FSM.GetState<CloseDistanceBehaviour>();
                closeDistance.SetTarget(skillNode.transform, minDistance, GetType());
                return typeof(CloseDistanceBehaviour);
            }
            player.Animator.animator.SetBool(actionAnimationName, true);
            player.CharacterLookAt(skillNode.transform.position - player.transform.position);
            actionTimer += dt;
            if (actionTimer > actionCooldown)
            {
                DoAction();
                actionTimer = 0f;
            }
            return GetType();
        }

        void DoAction()
        {
            var nodeData = skillNode.NodeData as ActionNodeData;
            var progressInfo = Services.Container.Resolve<SkillService>().GetActionProgress(skill, nodeData);
            actionCooldown = progressInfo.Cooldown;
            skillNode?.TakeDamage(progressInfo.Hit ? progressInfo.Progress : 0);
        }

        public override void OnForceChange()
        {
            base.OnForceChange();
            player.Animator.animator.SetBool(actionAnimationName, false);
        }

        public override void SetInteractableNode(IInteractableNode node)
        {
            skillNode = node as MiningSkillNode;

            var hasRequirements = CheckRequirements(skillNode.NodeData);
            if (hasRequirements)
            {
                EquipAxeVisuals();
                var progressInfo = Services.Container.Resolve<SkillService>().GetActionProgress(skill, skillNode.NodeData as ActionNodeData);
                actionCooldown = progressInfo.Cooldown;
            }
        }

        void EquipAxeVisuals()
        {
            var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
            var equippedItem = profile.PlayerEquipmentManager.GetItemOnSlot(toolSlot);
            player.PlayerItem3DVisuals.Enable(equippedItem as Equipment);
        }

        bool CheckRequirements(NodeData nodeData)
        {
            var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
            var currentTool = profile.PlayerEquipmentManager.GetItemOnSlot(toolSlot);
            if (currentTool == null)
            {
                OverlayCanvas.Instance.ToastNotificationContainer.Show(new TextLayout.LayoutData
                { 
                    LocalizedText = "You need to equip a pickaxe to mine here",
                });
                player.ResetFSM();
                return false;
            }

            var requirementsResponse = profile.MeetsRequirement(nodeData.requirement);
            if (!requirementsResponse.HasRequirements)
            {
                OverlayCanvas.Instance.ToastNotificationContainer.Show(new TextLayout.LayoutData
                { 
                    LocalizedText = requirementsResponse.LocalizedReasonForFail,
                });
                player.ResetFSM();
                return false;
            }

            return true;
        }

        bool NeedsToCloseInDistance(Transform target)
        {
            return Vector3.Distance(target.position, player.transform.position) > minDistance ;
        }
    }
}