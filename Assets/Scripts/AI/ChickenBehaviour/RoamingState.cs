using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class RoamingState : FSMBaseState
{
    private Enemy enemy;
    private Player player;
    private Vector3 origin;

    float nextDirectionChangeTime = 0f;
    Vector3 direction;

    public RoamingState(Enemy enemy, Player player, Vector3 origin)
    {
        this.enemy = enemy;
        this.player = player;
        this.origin = origin;
    }

    public override Type Update(float dt)
    {
        if (Time.time > nextDirectionChangeTime)
        {
            if (Random.value < .6f)
            {
                var randomDirection = Random.insideUnitCircle;
                direction.x = randomDirection.x;
                direction.z = randomDirection.y;
                direction.y = 0;
            }
            else
            {
                direction = Vector3.zero;
            }
            nextDirectionChangeTime = Time.time + 1 + Random.value * 2;
        }

        var speedMulti = 1f;
        var distanceFromOrigin = Vector3.Distance(enemy.transform.position.HorizontalPlane(), origin.HorizontalPlane());
        if (distanceFromOrigin > 3f)
        {
            direction = origin - enemy.transform.position;
            nextDirectionChangeTime = Time.time + .2f;
            speedMulti = Mathf.InverseLerp(0, 3, distanceFromOrigin - 3) * 5;
            speedMulti = Mathf.Clamp(speedMulti, 1, 5);
        }
        
        if (direction == Vector3.zero)
            return GetType();

        if (NavMesh.SamplePosition(enemy.transform.position + direction.normalized * .1f, out var hit, .1f, NavMesh.AllAreas))
        {
            enemy.Move(direction.normalized * speedMulti);
            enemy.LookAt(direction.normalized);   
        }
        return GetType();
    }
}




