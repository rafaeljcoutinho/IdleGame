using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physics_2p5D
{
    [Serializable]
    public class RigidbodyParams
    {
        public LayerMask Floor;
        public LayerMask Obstacles;
        public bool CollideAndSlide;
        public float horizontalDrag;
        public float verticalDrag;
    }

    public class Rigidbody
    {
        private Vector3 acceleration;
        private Vector3 velocity;
        private Vector3 position;

        private Transform t;
        private CapsuleCollider myCollider;
        private bool grounded;
        public Vector3 Position => position;
        public Vector3 Velocity => velocity;
        public bool Grounded => grounded;

        private Vector3 lastPosition;
        private RaycastHit[] buffer = new RaycastHit[16];
        private Collider[] bufferColliders = new Collider[16];
        
        private List<Vector3> velocityChanges = new (16);
        RigidbodyParams rigidbodyParams;
        private List<ForceOverTime> forceOverTimes;

        public RigidbodyParams RigidbodyParams => rigidbodyParams;
        public bool HasDrag { get; set; }

        public Rigidbody(Transform t, CapsuleCollider myCollider, RigidbodyParams rigidbodyParams)
        {
            position = t.position;
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
            this.rigidbodyParams = rigidbodyParams;
            this.t = t;
            this.myCollider = myCollider;
            forceOverTimes = new();
        }

        public void AddVelocityChange(Vector3 change)
        {
            velocityChanges.Add(change);
        }
        
        public void AddForce(Vector3 f)
        {
            acceleration += f;
        }

        public void StopVerticalMovement()
        {
            velocity.y = 0;
        }

        public void RestPosition(Vector3 position, bool resetHeight)
        {
            if (resetHeight)
            {
                this.position.y = position.y;
            }

            this.position.x = position.x;
            this.position.z = position.z;
        }
        
        public void StopHorizontal()
        {
            velocity.x = 0;
            velocity.z = 0;
        }

        public void Step(float dt)
        {
            lastPosition = position;
            var forceOverTimeDisplacement = AddForcesOverTime();
            if (HasDrag)
                ApplyDrag(dt);
            else
                StopHorizontal();

            var displacementVector = GetIntegratedDisplacementVector(dt);
            var horizontalDisplacement = forceOverTimeDisplacement.HorizontalPlane() != Vector3.zero
                ? forceOverTimeDisplacement.HorizontalPlane()
                : displacementVector.HorizontalPlane();

            if (rigidbodyParams.CollideAndSlide)
            {
                MoveRecursive(horizontalDisplacement, horizontalDisplacement, 0);
            }
            else
            {
                position += horizontalDisplacement;
            }
            ApplyGroundChecks(forceOverTimeDisplacement.y == 0 ? displacementVector.y : forceOverTimeDisplacement.y);
            if (Grounded)
                velocity.y = 0;
            Depenetrate();
            acceleration = Vector3.zero;
            HasDrag = false;
        }
        
        private Vector3 AddForcesOverTime()
        {
            var ans = Vector3.zero;
            for (var i = forceOverTimes.Count - 1; i >= 0; i--)
            {
                var f = forceOverTimes[i];
                f.frames++;

                if (f.animationCurve != null)
                {
                    var elapsedRatio = f.frames / (float)f.duration;
                    var force = f.animationCurve.Evaluate(elapsedRatio) * f.force;
                    ans += force * f.direction.normalized;
                }
                else
                {
                    ans +=f.force * f.direction.normalized;
                }
                if (f.frames >= f.duration)
                {
                    forceOverTimes.RemoveAt(i);
                    ForceOverTimeCache.Instance.Cache.Return(f);
                }
            }

            return ans;
        }
        
        public void StopForceOverTime(int source)
        {
            for (var i = forceOverTimes.Count - 1; i >= 0; i--)
            {
                var f = forceOverTimes[i];
                if (f.source == source)
                {
                    forceOverTimes.RemoveAt(i);
                }
            }
        }

        Vector3 GetIntegratedDisplacementVector(float dt)
        {
            velocity += acceleration * dt;
            return velocity * dt;
        }
        
        private void Depenetrate()
        {
            var r = myCollider.radius;
            var h = myCollider.height;
            var colliders = Physics.OverlapCapsuleNonAlloc(position + Vector3.up * (r + .1f),
                position + Vector3.up * (h + .1f - r), r+.1f, bufferColliders, rigidbodyParams.Floor | rigidbodyParams.Obstacles);

            for (var i =0; i < colliders; i++)
            {
                if (bufferColliders[i] == myCollider)
                    continue;
                if (Physics.ComputePenetration(bufferColliders[i], bufferColliders[i].transform.position, bufferColliders[i].transform.rotation,
                    myCollider, position, myCollider.transform.rotation,
                    out var direction, out var distance))
                {
                    position -= direction * (distance * 1.1f);
                }
            }
        }

        void MoveRecursive(Vector3 originalDirection, Vector3 displacement, int iter)
        {
            if (iter == 6) return;
            if (Vector3.Angle(originalDirection, displacement) > 90) return;

            var center =  position + myCollider.center;
            var height = myCollider.height;
            var r = myCollider.radius * .95f;
            var p1 = center + myCollider.transform.up * (height / 2f - r);
            var p2 = center - myCollider.transform.up * (height / 2f - (r + .15f));

            var n = Physics.CapsuleCastNonAlloc(p1, p2, r, displacement.normalized, buffer, displacement.magnitude, rigidbodyParams.Floor | rigidbodyParams.Obstacles);

            var closestHit = -1;
            for (var i = 0; i < n; i++)
            {
                if (buffer[i].collider == myCollider) continue;
                if (closestHit == -1 || buffer[i].distance < buffer[closestHit].distance && buffer[closestHit].distance != 0)
                    closestHit = i;
            }

            var toDisplace = displacement.magnitude;
            if (closestHit != -1)
            {
                toDisplace = buffer[closestHit].distance - r;
            }

            toDisplace = Mathf.Clamp(toDisplace, 0, float.MaxValue);
            position += displacement.normalized * toDisplace;
            var remainingDisplacement = displacement.magnitude - toDisplace;
            if (remainingDisplacement > .001f)
            {
                var reflection = Vector3.Reflect(displacement, buffer[closestHit].normal);
                var projection = Vector3.ProjectOnPlane(reflection, buffer[closestHit].normal);
                MoveRecursive(originalDirection, projection.normalized * remainingDisplacement, iter + 1);
            }
        }
        
        public void AddForceOverTime(ForceOverTime f)
        {
            forceOverTimes.Add(f);
        }
        
        public bool HasForceOverTime(int source)
        {
            foreach (var f in forceOverTimes)
            {
                if (f.source == source) return true;
            }

            return false;
        }

        public float DistanceToGround()
        {
            RaycastHit hit;
            var r = myCollider.radius;
            var isHit = Physics.SphereCast(position + Vector3.up * (r + .1f), r,Vector3.down,out hit, float.MaxValue, rigidbodyParams.Floor | rigidbodyParams.Obstacles);
            if (!isHit)
            {
                return float.MaxValue;
            }
            return hit.distance;
        }
        
        void ApplyGroundChecks(float displacement)
        {
            if (displacement > 0)
            {
                grounded = false;
                position.y += displacement;
                return;
            }

            RaycastHit hit;
            var r = myCollider.radius;
            var isHit = Physics.SphereCast(position + Vector3.up * (r + .1f), r,Vector3.down,out hit, Mathf.Abs(displacement) + .1f + r, rigidbodyParams.Floor | rigidbodyParams.Obstacles);

            if (isHit)
            {
                position.y = hit.point.y;
                velocity.y = 0;
                grounded = true;
            }
            else
            {
                grounded = false;
                position.y += displacement;
            }
        }

        public void ApplyDrag(float dt)
        {
            var verticalDrag = -velocity.y * rigidbodyParams.verticalDrag;
            if (Math.Abs(verticalDrag * dt) > Math.Abs(velocity.y))
            {
                verticalDrag = -velocity.y;
            }
            
            var horizontalDrag = -velocity.HorizontalPlane() * rigidbodyParams.horizontalDrag;
            if (Mathf.Abs(horizontalDrag.magnitude * dt) > Mathf.Abs(velocity.HorizontalPlane().magnitude))
            {
                horizontalDrag = -velocity.HorizontalPlane();
            }

            Vector3 ans = new Vector3(horizontalDrag.x, verticalDrag, horizontalDrag.z);
            AddForce(ans);
        }

        public void RestartGravity()
        {
            acceleration.y = 0;
            velocity.y = 0;
        }
    }
}