using System.Collections.Generic;
using UnityEngine;

public class DamageInfo : IPoolable
{
    public HitInfo hitInfo;
    public List<Collider> targets;
    public IDamageReceiver target;

    public void OnCreateIPoolable()
    {
        hitInfo = new HitInfo();
        hitInfo.Reset();
        targets = new List<Collider>();
    }

    public void OnEnableIPoolable() {}

    public void OnDisableIPoolable()
    {
        hitInfo.Reset();
        targets.Clear();
        target = null;
    }
}