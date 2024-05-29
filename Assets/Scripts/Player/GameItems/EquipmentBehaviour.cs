using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentBehaviour : MonoBehaviour
{
    protected Equipment equipment;
    protected List<Enemy> enemyManager;
    protected Player player;
    public bool Enabled { get; private set; }

    public virtual void SetEnemies(List<Enemy> enemies)
    {
        this.enemyManager = enemies;
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
    }

    public virtual void SetTarget(Enemy enemy) {}
    public virtual void StartAttack(Action onHit) {}
    public virtual void UpdateBehavior(float dt) {}

    public virtual void Enable()
    {
        gameObject.SetActive(true);
        Enabled = true;
    }
    
    public virtual void Disable()
    {
        gameObject.SetActive(false);
        Enabled = false;
    }

    public virtual void Bind(Equipment equipment)
    {
        this.equipment = equipment;
    }
    public virtual void OnEquip() {}
    public virtual void OnUnequip() {}
    public virtual void PostDamage(DamageInfo damageInstances) {}
    public virtual void DamageTaken(HitInfo damageInstances) {}
}
