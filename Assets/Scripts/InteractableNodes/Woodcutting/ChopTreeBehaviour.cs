using System;
using System.Collections.Generic;
using UnityEngine;

namespace Woodcutting
{
    public class ChopTreeBehaviour : Player.Behaviour
    {
        public const float minDistance = .5f;
        private float choppingCooldown = 0f;
        private float waitTimer = 0;
        private Player player;

        private List<BaseInteractableNodeMonobehaviour> trees;
        private Tree currentTree;
        public override SkillData.Type SkillUsed => SkillData.Type.Woodcutting;
        public override bool AllowFSMReset => false;

        public ChopTreeBehaviour(Player player, FSM<Player.Behaviour> fsm)
        {
            this.player = player;
            FSM = fsm;
            player.AnimationEventDispatcher.OnChopAnimationEvent += DoChop;
        }

        public override Type Update(float dt)
        {
            waitTimer += dt;
            if (waitTimer < .3f)
            {
                return GetType();
            }

            if (currentTree == null || currentTree.IsRespawning)
            {
                currentTree?.DeHighlight();
                currentTree = ClosestTreeNotRespawning();
                if (currentTree != null)
                {
                    choppingCooldown = GetCooldown();
                    currentTree.Highlight();
                }
                player.Animator.animator.SetBool("chopping", false);
            }

            if (currentTree == null)
            {
                return GetType();
            }

            if (NeedsToCloseInDistance(currentTree))
            {
                player.Animator.animator.SetBool("chopping", false);
                var closeDistance = FSM.GetState<CloseDistanceBehaviour>();
                closeDistance.SetTarget(currentTree.transform, minDistance, GetType());
                return typeof(CloseDistanceBehaviour);
            }
            waitTimer = 0;
            player.Animator.animator.SetBool("chopping", true);
            player.Animator.animator.SetFloat("chopping_speed", GetAdjustAnimationSpeed());
            player.CharacterLookAt(currentTree.transform.position - player.transform.position);
            return GetType();
        }

        float GetCooldown()
        {
            var nodeData = currentTree.NodeData as ActionNodeData;
            var cd = Services.Container.Resolve<SkillService>().GetActionProgress(SkillData.Type.Woodcutting, nodeData).Cooldown;
            return cd;
        }

        float GetAdjustAnimationSpeed()
        {
            var duration = player.Animator.GetAnimationDuration("MeleeAttack_TwoHanded");
            return duration/choppingCooldown;
        }
        
        void DoChop()
        {
            var nodeData = currentTree.NodeData as ActionNodeData;
            var progressInfo = Services.Container.Resolve<SkillService>().GetActionProgress(SkillData.Type.Woodcutting, nodeData);
            choppingCooldown = progressInfo.Cooldown;
            currentTree?.TakeDamage(progressInfo.Hit? progressInfo.Progress : 0);
        }

        public override void OnForceChange()
        {
            base.OnForceChange();
            player.Animator.animator.SetBool("chopping", false);
            currentTree?.DeHighlight();
        }

        public override void SetInteractableNode(IInteractableNode node)
        {
            var treeNode = (node as Tree);
            waitTimer = 1f;
            SetTreesToChop(treeNode.Trees);
            var hasRequirements = CheckRequirements(treeNode);
            if (hasRequirements)
            {
                EquipAxeVisuals();
                currentTree = treeNode;
                choppingCooldown = GetCooldown();
            }
        }

        void EquipAxeVisuals()
        {
            var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
            var equippedAxe = profile.PlayerEquipmentManager.GetItemOnSlot(Equipment.EquipSlot.ToolBeltAxe);
            player.PlayerItem3DVisuals.Enable(equippedAxe as Equipment);
        }

        bool CheckRequirements(Tree tree)
        {
            var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
            var currentAxe = profile.PlayerEquipmentManager.GetItemOnSlot(Equipment.EquipSlot.ToolBeltAxe);
            if (currentAxe == null)
            {
                OverlayCanvas.Instance.ToastNotificationContainer.Show(new TextLayout.LayoutData
                { 
                    LocalizedText = "You need to equip an axe to chop trees",
                });
                player.ResetFSM();
                return false;
            }

            var requirementsResponse = profile.MeetsRequirement(tree.NodeData.requirement);
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

        public void SetTreesToChop(List<BaseInteractableNodeMonobehaviour> trees)
        {
            this.trees = trees;
            currentTree = null;
        }

        bool NeedsToCloseInDistance(Tree tree)
        {
            return Vector3.Distance(tree.transform.position, player.transform.position) > minDistance ;
        }

        Tree ClosestTreeNotRespawning()
        {
            Tree closest = null;
            foreach (var node in trees)
            {
                var tree = node as Tree;
                if (tree.IsRespawning) continue;
                closest = tree;
            }

            if (closest == null)
            {
                return null;
            }
            
            var closestDistance = Vector3.Distance(player.transform.position, closest.transform.position);
            
            for (var i = 0; i < trees.Count; i++)
            {
                var tree = trees[i] as Tree;
                if (tree.IsRespawning)
                    continue;
                var d = Vector3.Distance(player.transform.position, trees[i].transform.position);
                if (d < closestDistance)
                {
                    closestDistance = d;
                    closest = tree;
                }
            }

            return closest;
        }

    }
}