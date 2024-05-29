using UnityEngine;

public class CounterStrikeEquipment : EquipmentBehaviour, IDamageSource
{
    public IDamageReceiver damageReceiver => player;

    public override void DamageTaken(HitInfo damageInstances)
    {
        base.DamageTaken(damageInstances);
        var hitParams = equipment.BehaviourParams as OnHitParams;
        DoDamage(damageInstances.damageSource.damageReceiver, hitParams);
    }
    
    private void DoDamage(IDamageReceiver source, OnHitParams onhitParams)
    {
        if (Random.value > onhitParams.chanceForDamage)
        {
            return;
        }
        var pooledDamageInfo = Services.Container.Resolve<DamageProcessorService>().GetPooledDamageInfo();
        pooledDamageInfo.hitInfo.Damage = onhitParams.damage;
        pooledDamageInfo.target = source;
        pooledDamageInfo.hitInfo.Hit = true;
        pooledDamageInfo.hitInfo.damageSource = this;
    }
}