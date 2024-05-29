using UnityEngine;

public struct HitInfo
{
    public int Damage;
    public Vector3 Knockback;
    public float StunDuration;
    public Vector3 HitOrigin;
    public IDamageSource damageSource;
    public bool IsCritical;
    public bool Hit;
    public bool TriggersOnHits;

    public void Reset()
    {
        damageSource = null;
        Damage = 0;
        Knockback = Vector3.zero;
        StunDuration = 0;
        IsCritical = false;
        Hit = false;
        TriggersOnHits = false;
    }
}