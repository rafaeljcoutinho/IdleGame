using System;
using UnityEngine;
using UnityEngine.AI;

namespace Woodcutting
{
    public class CloseDistanceBehaviour : Player.Behaviour
    {
        private Player player;
        private Transform target;
        private Vector3 targetPosition;
        private float minDistance;
        private Type nextState;
        public override SkillData.Type SkillUsed => FSM.GetState(nextState).SkillUsed;
        public override bool AllowFSMReset => false;
        private NavMeshPath path = new NavMeshPath();
        private Vector3[] corners;
        private int currentCorner;

        private Action callback;

        public CloseDistanceBehaviour(Player player, FSM<Player.Behaviour> fsm)
        {
            this.player = player;
            FSM = fsm;
        }

        public void OnDrawGizmos()
        {
            var targetP = target == null ? targetPosition : target.transform.position;
            var direction = (targetP - player.transform.position).normalized;
            Gizmos.DrawLine(player.transform.position, targetP);
            var avoidDir = GetAvoidanceDirection(player, target, direction);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(player.transform.position, player.transform.position + avoidDir);
        }
        
        public override Type Update(float dt)
        {
            var targetP = target == null ? targetPosition : target.transform.position;
            var distance = Vector3.Distance(player.transform.position, targetP);
            if (distance < minDistance)
            {
                callback?.Invoke();
                callback = null;
                return nextState;
            }

            if (path == null || corners.Length == 0 || distance < 1f || currentCorner >= corners.Length)
            {
                var direction = (targetP - player.transform.position).normalized;
                //var avoidDirection = GetAvoidanceDirection(player, target, direction);
                //direction += avoidDirection;
                var projectedOnGround = direction.HorizontalPlane().normalized;
                player.MoveAndRotateCharacter(projectedOnGround);   
            }
            else
            {
                var direction = corners[currentCorner] - player.transform.position;
                if (direction.magnitude < .5f)
                {
                    currentCorner += 1;
                }
                direction.Normalize();
                //direction += GetAvoidanceDirection(player, target, direction);
                var projectedOnGround = direction.HorizontalPlane().normalized;
                player.MoveAndRotateCharacter(projectedOnGround);
            }
            return GetType();
        }

        public static Vector3 GetAvoidanceDirection(Player player, Transform target, Vector3 wantedDirection)
        {
            RaycastHit hit;
            var castRadius = player.CharacterController.Collider.radius * 2f;
            var layer = player.CharacterController.Rb.RigidbodyParams.Obstacles;
            var castOrigin = player.transform.position + player.transform.up * 1.5f * castRadius;
            if (Physics.SphereCast(castOrigin, castRadius, wantedDirection, out hit, 1f, layer))
            {
                if (hit.transform == target) 
                    return Vector3.zero;

                if (Vector3.SignedAngle(wantedDirection, hit.normal, Vector3.up) > -10)
                {
                    return Vector3.Cross(hit.normal, Vector3.up).normalized;
                }
                else
                {
                    return Vector3.Cross(Vector3.up, hit.normal).normalized;
                }
            }
            return Vector3.zero;
        }

        public void SetTargetPosition(Vector3 position, float minDistance, Type nextState, Action callback = null)
        {
            this.callback = callback;
            if (Vector3.Distance(player.transform.position, position) > 3f)
            {
                path = new NavMeshPath();
                NavMesh.CalculatePath(player.transform.position, position, NavMesh.AllAreas, path);
                corners = path.corners;
                currentCorner = 0;
            }
            else
            {
                path = null;
            }

            target = null;
            targetPosition = position;
            this.minDistance = minDistance;
            this.nextState = nextState;
        }
        
        public void SetTarget(Transform t, float minDistance, Type nextState, Action callback = null)
        {
            this.callback = callback;
            if (Vector3.Distance(player.transform.position, t.transform.position) > 3f)
            {
                path = new NavMeshPath();
                NavMesh.CalculatePath(player.transform.position, t.transform.position, NavMesh.AllAreas, path);
                corners = path.corners;
                currentCorner = 0;
            }
            else
            {
                path = null;
            }
            
            target = t;
            this.minDistance = minDistance;
            this.nextState = nextState;
        }
        
        public override void SetInteractableNode(IInteractableNode node)
        {
        }
    }
}