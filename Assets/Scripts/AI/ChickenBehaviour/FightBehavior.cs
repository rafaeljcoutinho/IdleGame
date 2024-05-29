using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FightBehavior : FSMBaseState
{
    private Enemy enemy;
    private Player target;
    private float hitDist;
    private float attackCD = 2f;
    private float attackTimer = 0f;
    private float waitTimer = 10f;
    private List<Collider> targets = new (4);
    private int damage;
    private float deAggroDistance = 4;

    public override Type Update(float dt)
    {
        if (target == null || Vector3.Distance(enemy.transform.position, target.transform.position) > deAggroDistance)
        {
            return typeof(RoamingState);   
        }

        if (!target.HpController.IsAlive)
        {
            return typeof(RoamingState);
        }

        waitTimer += dt;
        enemy.transform.LookAt(target.transform);
        var dist = (target.transform.position - enemy.transform.position).magnitude;
        hitDist = .65f;
        if(dist < hitDist)
        {
            if(enemy.IsAlive)
                TryExecuteHit(dt);
        }
        else
        {
            if(waitTimer >= .2f)
                GetCloser(dt);
        }
        return GetType();
    }

    private void GetCloser(float dt)
    {
        var direction = target.transform.position.HorizontalPlane() - enemy.transform.position.HorizontalPlane();
        direction += GetAvoidanceDirection(direction);
        var speedMultiplier = 2;
        enemy.Move(direction.HorizontalPlane().normalized * speedMultiplier);
    }

    Vector3 GetAvoidanceDirection(Vector3 wantedDirection)
    {
        RaycastHit hit;
        var castRadius = enemy.Controller.Collider.radius;
        var layer = enemy.Controller.Rb.RigidbodyParams.Obstacles | (1 << enemy.gameObject.layer);
        var castOrigin = enemy.transform.position + enemy.transform.up * 1.5f * castRadius;

        if (Physics.SphereCast(castOrigin, castRadius, wantedDirection, out hit, 1f, layer))
        {
            if (hit.transform == target) return Vector3.zero;
            Vector3 avoidanceDirection = Vector3.Cross(hit.normal, Vector3.up).normalized;
            return avoidanceDirection;
        }
        return Vector3.zero;
    }
    private void TryExecuteHit(float dt)
    {
        if (Time.time >= attackTimer)
        {
            enemy.Animator.SetTrigger("Attack");
            waitTimer = 0.2f;
            attackTimer = Time.time + attackCD;
        }
    }


    private void ApplyDamage()
    {
        if (target == null)
            return;
        target.TakeDamage( new HitInfo
        {
            Damage = Mathf.RoundToInt(Random.Range(damage * .8f, damage * 1.1f)),
            damageSource = enemy,
        });
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }
    
    public FightBehavior(Enemy enemy , AnimationEventDispatcher animationEvent , int damage)
    {
        this.damage = damage;
        this.enemy = enemy;
        animationEvent.OnAttackAnimationEvent += ApplyDamage;
        attackTimer = Time.time;
    }

    public void SetTarget(Player target)
    {
        waitTimer = .2f;
        this.target = target;
    }


}
