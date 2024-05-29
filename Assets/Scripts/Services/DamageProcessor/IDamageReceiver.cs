using UnityEngine;
public interface IDamageReceiver
{
    public Collider Collider { get; }
    public Transform transform { get; }
    public SkillService.OverkillInfo TakeDamage(HitInfo hitInfo);
    public EnemyHpController HpController { get; }
    public Enemy Enemy { get; }
}