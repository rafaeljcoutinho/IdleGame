using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Woodcutting
{
    public class Tree : BaseInteractableNodeMonobehaviour
    {
        [SerializeField] private MeshRenderer meshRendererTree;
        [SerializeField] private MeshRenderer meshRendererStump;
        [SerializeField] private InteractableNodeGroup treeGroup;
        [SerializeField] private EnemyHpController hpController;
        [SerializeField] private ActionNodeData nodeData;
        [SerializeField] private Transform dropAnchor;

        public override NodeData NodeData => nodeData;
        public List<BaseInteractableNodeMonobehaviour> Trees => treeGroup?.Nodes ?? new List<BaseInteractableNodeMonobehaviour> { this };
        public bool IsRespawning { private set; get; }

        private void Awake()
        {
            hpController.SetMaxHp(nodeData.hp);
            hpController.ResetHp();
            hpController.OnDeath += OnDeath;
        }

        public override Type PlayerBehaviour => typeof(ChopTreeBehaviour);

        public void TakeDamage(float damage)
        {
            hpController.TakeDamage(new HitInfo
            {
                Damage = (int)damage,
            });
            transform.DOPunchScale(transform.up * .1f, .2f);
        }

        private void OnDeath(SkillService.OverkillInfo overkillInfo)
        {
            var instaRespawn = Random.value < nodeData.chanceToAutoRespawn;
            Services.Container.Resolve<OverkillService>().NotifyEnemyDied(nodeData);
            DropFromDroptable(SkillData.Type.Woodcutting, hpController, nodeData, dropAnchor);
            if (instaRespawn)
            {
                hpController.ResetHp();
            }
            else
            {
                meshRendererTree.enabled = false;
                meshRendererStump.enabled = true;
                IsRespawning = true;
                hpController.ResetHp();
                StartCoroutine(AfterDelay());
            }
            
        }

        private IEnumerator AfterDelay()
        {
            yield return new WaitForSeconds(nodeData.respawnCooldown);
            meshRendererTree.enabled = true;
            meshRendererStump.enabled = false;
            IsRespawning = false;
        }
    }
}