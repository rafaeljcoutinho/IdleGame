using System.Collections.Generic;
using UnityEngine;

public class DOTAuraBehavior : EquipmentBehaviour, IDamageSource
{
    private float damageTimer = 0;
    public IDamageReceiver damageReceiver => player;

    public override void UpdateBehavior(float dt)
    {
        base.UpdateBehavior(dt);
        damageTimer += dt;
        var auraParams = equipment.BehaviourParams as DotAuraParams;

        var interval = 1 / auraParams.FreqHz;
        if (damageTimer > interval)
        {
            damageTimer -= interval;
            var targets = new List<Collider>();
            foreach (var enemy in enemyManager)
            {
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }
                
                if (Vector3.Distance(enemy.transform.position, player.transform.position) < auraParams.Range)
                {
                    targets.Add(enemy.HpController.Collider);
                }
            }
            if (targets.Count > 0)
            {
                var pooledDamageInfo = Services.Container.Resolve<DamageProcessorService>().GetPooledDamageInfo();
                pooledDamageInfo.hitInfo.Damage = auraParams.Damage;
                pooledDamageInfo.targets = targets;
                pooledDamageInfo.hitInfo.Hit = true;
                pooledDamageInfo.hitInfo.damageSource = this;
            }
        }
    }
}
