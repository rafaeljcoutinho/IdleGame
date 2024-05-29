using UnityEngine;
public interface IDamageSource
{
    public IDamageReceiver damageReceiver { get; }
}