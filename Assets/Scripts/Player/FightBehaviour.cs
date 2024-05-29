using System;
using UnityEngine;
using Woodcutting;

[Serializable]
public class FightBehaviour : Player.Behaviour
{
    private Player player;
    private bool isInAttackAnimation;
    private float nextActionTimer;
    private float stopMovementTimer;
    private EnemyManager enemySpawner;
    private Enemy currentTarget;
    private Enemy potentialNew;
    private Equipment weapon;
    bool IsAutoFightOn => OverlayCanvas.Instance.Hud.AutoFightEnabled;
    public override SkillData.Type SkillUsed => weapon == null ? SkillData.Type.Melee : weapon.MainType; 
    public override bool AllowFSMReset => false;
    private bool isClosingDistance = false;

    public FightBehaviour(Player player)
    {
        this.player = player;
        this.player = player;
    }
    
    private ActionProgressParameters GetAttackParams()
    {
        weapon = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.GetItemOnSlot(Equipment.EquipSlot.MainHand) as Equipment;
        var targetNodeData = currentTarget?.NodeData as ActionNodeData ?? null;
        var combinedParams = Services.Container.Resolve<SkillService>().GetCombinedProgressParameters(SkillUsed, targetNodeData);
        return combinedParams;
    }

    public override void OnForceChange()
    {
        currentTarget?.DeHighlight();
    }

    public override Type Update(float dt)
    {
        var playerIsTryingToMove = player.Inputs != Vector3.zero;
        var canMove = Time.time >= stopMovementTimer;

        var weaponParams = GetAttackParams();

        var attackOffCd = Time.time >= nextActionTimer;
        var hasNoTarget = currentTarget == null || !currentTarget.IsAlive;
        if (hasNoTarget && !attackOffCd)
        {
            potentialNew = ClosestEnemy();
            potentialNew?.Highlight();
            var shouldMoveCloserToTarget = !playerIsTryingToMove && IsAutoFightOn;
            if (potentialNew != null && canMove && shouldMoveCloserToTarget)
            {
                var dist = Vector3.Distance(player.transform.position.HorizontalPlane(), potentialNew.transform.position.HorizontalPlane());
                var walkDistance = dist - weaponParams.Range * .8f;
                if (isClosingDistance && walkDistance <= 0f)
                {
                    isClosingDistance = false;
                }
                if (!isClosingDistance && walkDistance > .5f)
                {
                    isClosingDistance = true;
                }
                if (walkDistance > 0 && isClosingDistance)
                {
                    var direction = potentialNew.transform.position.HorizontalPlane() - player.transform.position.HorizontalPlane();
                    var projectedOnGround = direction.HorizontalPlane().normalized;
                    player.MoveAndRotateCharacter(projectedOnGround);
                }
            }
        }
        if (attackOffCd && hasNoTarget)
        {
            currentTarget = ClosestEnemy();
            currentTarget?.Highlight();
        }
        
        var equipBehavior = weapon == null ? player.DefaultFightBehavior :player.PlayerItem3DVisuals.GetBehavior(weapon);
        equipBehavior.SetTarget(currentTarget);
        if (currentTarget != null && currentTarget.IsAlive)
        {
            var attackCmd = player.Interact || (!playerIsTryingToMove && IsAutoFightOn);
            var targetStillInRange = Vector3.Distance(player.transform.position.HorizontalPlane(), currentTarget.transform.position.HorizontalPlane()) < weaponParams.Range * .8f;
            var canAttack = attackOffCd && targetStillInRange && attackCmd;
            
            if (canAttack)
            {
                StartAttack(weaponParams);
                canMove = Time.time >= stopMovementTimer;
            }

            var shouldMoveCloserToTarget = !playerIsTryingToMove && IsAutoFightOn;
            if (canMove && shouldMoveCloserToTarget && !targetStillInRange)
            {
                var direction = currentTarget.transform.position.HorizontalPlane() - player.transform.position.HorizontalPlane();
                direction += CloseDistanceBehaviour.GetAvoidanceDirection(player, currentTarget.transform, direction);
                var projectedOnGround = direction.HorizontalPlane().normalized;
                player.MoveAndRotateCharacter(projectedOnGround);
            }
        }

        var lookTarget = currentTarget;
        if (currentTarget == null || !currentTarget.IsAlive)
        {
            lookTarget = potentialNew;
        }
        if ( (attackOffCd || isInAttackAnimation) && lookTarget != null && lookTarget.IsAlive)
        {
            player.CharacterLookAt(lookTarget.transform.position.HorizontalPlane() - player.transform.position.HorizontalPlane(), 300000f);
        }
        return GetType();
    }

    Enemy ClosestEnemy()
    {
        Enemy closest = null;
        float closestDistance = float.MaxValue;
        foreach (var t in enemySpawner.Enemies)
        {
            var chicken = t;
            if (!chicken.IsAlive)
                continue;
            var distance = Vector3.SqrMagnitude(player.transform.position - chicken.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = chicken;
            }
        }

        return closest;
    }

    public override void SetInteractableNode(IInteractableNode node)
    {
        stopMovementTimer = 0;
        var chicken = node as Enemy;
        player.DefaultFightBehavior.SetEnemies(chicken.EnemySpawner.Enemies);
        enemySpawner = chicken.EnemySpawner;
        currentTarget = chicken;
        currentTarget.Highlight();
    }

    public void StartAttack(ActionProgressParameters parameters)
    {
        var behavior = weapon == null ? player.DefaultFightBehavior : player.PlayerItem3DVisuals.GetBehavior(weapon);
        behavior.StartAttack(AttackComplete);
        isInAttackAnimation = true;
        stopMovementTimer = float.MaxValue;
        nextActionTimer = Time.time + 1f / parameters.ActionFreqHz;
    }

    private void AttackComplete()
    {
        isInAttackAnimation = false;
        stopMovementTimer = Time.time + .2f;
    }
}