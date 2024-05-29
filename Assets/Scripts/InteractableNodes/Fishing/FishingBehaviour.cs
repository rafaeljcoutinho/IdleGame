using System;
using UnityEngine;
using Woodcutting;

namespace Fishing
{
    public class FishingBehaviour : Player.Behaviour
    {
        public const float minDistance = 1f;
        private float actionCooldown = 0f;
        private float actionTimer = 0f;
        private Player player;
        private FishingSpot fishSpot;
        public override SkillData.Type SkillUsed => SkillData.Type.Fishing;
        public override bool AllowFSMReset => false;

        private const string isActionAnimationName = "fishing";

        public FishingBehaviour(Player player, FSM<Player.Behaviour> fsm)
        {
            this.player = player;
            FSM = fsm;
        }

        public override Type Update(float dt)
        {
            if (fishSpot == null)
            {
                player.Animator.animator.SetBool(isActionAnimationName, false);
                return GetType();
            }

            if (NeedsToCloseInDistance(fishSpot.transform))
            {
                player.Animator.animator.SetBool(isActionAnimationName, false);
                var closeDistance = FSM.GetState<CloseDistanceBehaviour>();
                closeDistance.SetTarget(fishSpot.transform, minDistance, GetType());
                return typeof(CloseDistanceBehaviour);
            }
            
            
            player.Animator.animator.SetBool(isActionAnimationName, true);
            player.CharacterLookAt((fishSpot.transform.position - player.transform.position).HorizontalPlane());
            actionTimer += dt;
            if (actionTimer > actionCooldown)
            {
                DoFish();
                actionTimer = 0f;
            }
            return GetType();
        }

        void DoFish()
        {
            var nodeData = fishSpot.NodeData as ActionNodeData;
            var progressInfo = Services.Container.Resolve<SkillService>().GetActionProgress(SkillData.Type.Fishing, nodeData);
            actionCooldown = progressInfo.Cooldown;
            fishSpot?.TakeDamage(progressInfo.Hit ? progressInfo.Progress : 0);
        }

        public override void OnForceChange()
        {
            base.OnForceChange();
            player.Animator.animator.SetBool(isActionAnimationName, false);
            actionTimer = 0;
        }

        public override void SetInteractableNode(IInteractableNode node)
        {
            fishSpot = node as FishingSpot;

            var hasRequirements = CheckRequirements(fishSpot.NodeData);
            if (hasRequirements)
            {
                EquipAxeVisuals();
                var progressInfo = Services.Container.Resolve<SkillService>().GetActionProgress(SkillData.Type.Fishing, fishSpot.NodeData as ActionNodeData);
                actionCooldown = progressInfo.Cooldown;
            }
        }

        void EquipAxeVisuals()
        {
            var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
            var equippedAxe = profile.PlayerEquipmentManager.GetItemOnSlot(Equipment.EquipSlot.ToolBeltFishingRod);
            player.PlayerItem3DVisuals.Enable(equippedAxe as Equipment);
        }

        bool CheckRequirements(NodeData nodeData)
        {
            var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
            var currentAxe = profile.PlayerEquipmentManager.GetItemOnSlot(Equipment.EquipSlot.ToolBeltFishingRod);
            if (currentAxe == null)
            {
                OverlayCanvas.Instance.ToastNotificationContainer.Show(new TextLayout.LayoutData
                { 
                    LocalizedText = "You need to equip a fishing rod to fish here",
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