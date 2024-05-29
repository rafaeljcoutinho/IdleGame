using System;
using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;

public class SwordCastBehaviour : EquipmentBehaviour, IDamageSource
{
    [SerializeField] private string animationTrigger;
    [SerializeField] private int multiHitCount;
    [SerializeField] private int multiHitDelay;
    [SerializeField] private float multiHitDamagePercent;
    
    private List<Collider> hittedThings = new ();
    public IDamageReceiver damageReceiver { get; }
    private int multiHitCounter;
    private Action onHit;

    public override void Enable()
    {
        base.Enable();
//        player.AnimationEventDispatcher.OnAttackAnimationEvent += OnAttackAnimationHit;
        ChangeAttackHitboxRange();
        if (multiHitCount < 1)
        {
            multiHitCount = 1;
        }

        multiHitCounter = 0;
    }

    public override void Disable()
    {
        base.Disable();
        player.AnimationEventDispatcher.OnAttackAnimationEvent -= OnAttackAnimationHit;
    }

    private void ChangeAttackHitboxRange()
    {
        var combinedParams = Services.Container.Resolve<SkillService>().GetCombinedProgressParameters(equipment?.MainType ?? SkillData.Type.Melee, null);
        var center = player.AttackBehaviourParams.SwordHitRange.center;
        var size = player.AttackBehaviourParams.SwordHitRange.size;
        center.z = combinedParams.Range / 2;
        size.z = combinedParams.Range;
        player.AttackBehaviourParams.SwordHitRange.center = center;
        player.AttackBehaviourParams.SwordHitRange.size = size;
    }

    private void OnAttackAnimationHit()
    {
        player.AnimationEventDispatcher.OnAttackAnimationEvent -= OnAttackAnimationHit;
        onHit?.Invoke();
        PerformAndCacheHitRaycast();
        if (hittedThings.Count == 0)
            return;

        ShowHitVfx();
        DispatchDamageProcessor();
        TemporarySlowdownAnimation();
    }
    
    private void TemporarySlowdownAnimation()
    {
        player.AttackBehaviourParams.Animator.animator.speed = 0f;
        player.AttackBehaviourParams.SwordSlash.Pause();
        Services.Container.Resolve<CoroutineDispatcher>().AfterDelay(.1f, () =>
        {
            player.AttackBehaviourParams.Animator.animator.speed = 1;
            player.AttackBehaviourParams.SwordSlash.Play();
        });
    }

    private void ShowHitVfx()
    {
        for (int i = 0; i < hittedThings.Count; i++)
        {
            player.AttackBehaviourParams.HitFx.transform.position = hittedThings[i].ClosestPoint(
                player.transform.TransformPoint(player.AttackBehaviourParams.SwordHitRange.center));
            player.AttackBehaviourParams.HitFx.Play();
        }
    }

    private void DispatchDamageProcessor()
    {
        var damageProcessor = Services.Container.Resolve<DamageProcessorService>();
        multiHitCounter++;
        foreach (var hit in hittedThings)
        {
            var target = Services.Container.Resolve<DamageProcessorService>().GetDamageReceiver(hit);
            if (target == null || target.Enemy == null)
                continue;

            var actionProgress = Services.Container.Resolve<SkillService>().GetActionProgress(equipment?.MainType ?? SkillData.Type.Melee, target.Enemy.NodeData as ActionNodeData);
            var pooledDamageInfo = damageProcessor.GetPooledDamageInfo();
            var hitted = actionProgress.Hit;
            pooledDamageInfo.hitInfo.Damage = hitted ? Mathf.CeilToInt(actionProgress.Progress) : 0;
            pooledDamageInfo.targets.Add(hit);
            pooledDamageInfo.hitInfo.HitOrigin = player.transform.position;
            pooledDamageInfo.hitInfo.damageSource = player;
            pooledDamageInfo.hitInfo.IsCritical = actionProgress.IsCritical;
            pooledDamageInfo.hitInfo.Hit = actionProgress.Hit;
            pooledDamageInfo.hitInfo.TriggersOnHits = true;
            if (multiHitCounter == multiHitDelay)
            {
                for (var i = 0; i < multiHitCount; i++)
                {
                    pooledDamageInfo = damageProcessor.GetPooledDamageInfo();
                    hitted = actionProgress.Hit;
                    pooledDamageInfo.hitInfo.Damage = hitted ? Mathf.CeilToInt(actionProgress.Progress * multiHitDamagePercent) : 0;
                    pooledDamageInfo.targets.Add(hit);
                    pooledDamageInfo.hitInfo.HitOrigin = player.transform.position;
                    pooledDamageInfo.hitInfo.damageSource = player;
                    pooledDamageInfo.hitInfo.IsCritical = actionProgress.IsCritical;
                    pooledDamageInfo.hitInfo.Hit = actionProgress.Hit;
                    pooledDamageInfo.hitInfo.TriggersOnHits = true;
                }
                multiHitCounter = 0;
            }
        }
    }

    private Collider[] buffer = new Collider[16];
    private void PerformAndCacheHitRaycast()
    {
        var hitCount = Physics.OverlapBoxNonAlloc(player.transform.TransformPoint(player.AttackBehaviourParams.SwordHitRange.center),
            player.AttackBehaviourParams.SwordHitRange.size / 2,
            buffer,
            player.AttackBehaviourParams.SwordHitRange.transform.rotation,
            player.AttackBehaviourParams.AttackableLayer);

        hittedThings.Clear();
        if (hitCount > 0)
        {
            for (var i = 0; i < hitCount; i++)
            {
                if (player.AttackBehaviourParams.IgnoreColliders.Contains(buffer[i]))
                {
                    continue;
                }

                hittedThings.Add(buffer[i]);
            }
        }
    }

    public override void StartAttack(Action onHit)
    {
        player.AnimationEventDispatcher.OnAttackAnimationEvent += OnAttackAnimationHit;
        ChangeAttackHitboxRange();
        this.onHit = onHit;
        player.Animator.animator.SetTrigger(animationTrigger);
        player.AttackBehaviourParams.SwordSlash.Stop();
        player.AttackBehaviourParams.SwordSlash.Play();
    }
}
