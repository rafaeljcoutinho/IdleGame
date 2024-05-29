using System.Collections.Generic;
using UnityEngine;

public class OnHitBehavior : EquipmentBehaviour, IDamageSource
{
    public IDamageReceiver damageReceiver => player;

    public override void PostDamage(DamageInfo damageInstances)
    {
        var onHitParams = equipment.BehaviourParams as OnHitParams;
        base.PostDamage(damageInstances);
        if (!damageInstances.hitInfo.TriggersOnHits)
            return;

        if (onHitParams.doDamage)
        {
            DoDamage(damageInstances, onHitParams);
        }

        if (onHitParams.doHeal)
        {
            DoHeal(onHitParams);
        }
    }

    private void DoHeal(OnHitParams onhitParams)
    {
        if (Random.value > onhitParams.chanceForHeal)
        {
            return;
        }
        if (!onhitParams.usePercentHp)
        {
            player.HpController.Heal(onhitParams.flatHeal);
        }
        else
        {
            var maxHp = Mathf.CeilToInt(player.HpController.MaxHp * onhitParams.healPercentMaxHp);
            player.HpController.Heal(maxHp);
        }
    }

    private void DoDamage(DamageInfo damageInstances, OnHitParams onhitParams)
    {
        foreach (var target in damageInstances.targets)
        {
            var pooledDamageInfo = Services.Container.Resolve<DamageProcessorService>().GetPooledDamageInfo();
            var dmgReceiver = Services.Container.Resolve<DamageProcessorService>().GetDamageReceiver(target);
            if (dmgReceiver == null)
            {
                continue;
            }

            var finalDamage = Mathf.RoundToInt(dmgReceiver.HpController.MaxHp * onhitParams.hpDamagePercent);
            if (finalDamage < 1)
            {
                finalDamage = 1;
            }

            pooledDamageInfo.hitInfo.Damage = onhitParams.usePercent ? finalDamage : onhitParams.damage;
            pooledDamageInfo.targets = new List<Collider> { target };
            pooledDamageInfo.hitInfo.Hit = true;
            pooledDamageInfo.hitInfo.damageSource = this;
        }
    }
}
