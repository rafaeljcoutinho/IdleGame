using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamageProcessorService
{
    private Dictionary<Collider, IDamageReceiver> damageReceivers;
    private Pool<DamageInfo> damageInfoPool;
    public Pool<DamageInfo> DamageInfoPool => damageInfoPool;

    public DamageProcessorService()
    {
        damageReceivers = new Dictionary<Collider, IDamageReceiver>();
        damageInfoPool = new();
    }

    public void RegisterDamageReceiver(Collider collider, IDamageReceiver damageReceiver)
    {
        if (!damageReceivers.ContainsKey(collider))
        {
            damageReceivers.Add(collider, damageReceiver);
        }
    }

    public void StartDamageTick()
    {
        foreach (var dmg in damageInfoPool.InUse.ToList())
        {
            Return(dmg);
        }
    }

    public IDamageReceiver GetDamageReceiver(Collider collider)
    {
        if (!damageReceivers.ContainsKey(collider))
        {
            return null;
        }
        return damageReceivers[collider];
    }

    public void UnRegisterDamageReceiver(Collider collider)
    {
        if (collider != null && damageReceivers.ContainsKey(collider))
        {
            damageReceivers.Remove(collider);
        }
    }

    public DamageInfo GetPooledDamageInfo()
    {
        return damageInfoPool.GetOrCreate();
    }

    public void Return(DamageInfo damageInfo)
    {
        damageInfoPool.Return(damageInfo);
    }
    
    public void Return(List<DamageInfo> damageInfos)
    {
        foreach (var damageInfo in damageInfos)
        {
            damageInfoPool.Return(damageInfo);   
        }
    }

    public void ProcessDamagePool()
    {
        foreach (var damageInfo in damageInfoPool.InUse)
        {
            for (var i = 0; i < damageInfo.targets.Count; i++)
            {
                var target = damageInfo.targets[i];
                if (damageReceivers.TryGetValue(target, out var damageReceiver))
                {
                    damageReceiver.TakeDamage(damageInfo.hitInfo);
                    var dmgNumberView = OverlayCanvas.Instance.DisposableViewPool.Get<DamageNumberView>();
                    ShowDamageNumbers(damageReceiver, damageInfo.hitInfo, dmgNumberView);
                }
            }

            if (damageInfo.target != null)
            {
                damageInfo.target.TakeDamage(damageInfo.hitInfo);
                var dmgNumberView = OverlayCanvas.Instance.DisposableViewPool.Get<DamageNumberView>();
                ShowDamageNumbers(damageInfo.target, damageInfo.hitInfo, dmgNumberView);
            }
        }
    }
    
    private void ShowDamageNumbers(IDamageReceiver damageReceiver, HitInfo hitInfo,
        DamageNumberView damageNumberView)
    {
        var offset =  Vector3.up * 2f;
        var text = hitInfo.IsCritical && hitInfo.Damage > 0 ? $"<size=100%>{hitInfo.Damage}!</size>" : $"<size=60%>{hitInfo.Damage}";
        var color = hitInfo.IsCritical ? Color.yellow : Color.white;
        if (!hitInfo.Hit)
        {
            damageNumberView.Show(new DamageNumberView.ViewData
            {
                color = Color.white,
                text = $"<size=60%>dodge",
                spread = .0f,
            }, damageReceiver.transform, offset);
            OverlayCanvas.Instance.DisposableViewPool.Return(damageNumberView, .2f);
            return;
        }
        damageNumberView.Show(new DamageNumberView.ViewData
            {
                color = color,
                text = text,
                spread = .5f,
            }, 
            damageReceiver.transform, 
            offset);
        OverlayCanvas.Instance.DisposableViewPool.Return(damageNumberView, .6f);
    }
    
}