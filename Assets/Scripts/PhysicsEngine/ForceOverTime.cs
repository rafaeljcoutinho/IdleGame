using System;
using DefaultNamespace;
using UnityEngine;

namespace Physics_2p5D
{
    [Serializable]
    public class ForceOverTime : IPoolable
    {
        public AnimationCurve animationCurve;
        public int duration;
        public float force;

        [NonSerialized] public int source;
        [NonSerialized] public Vector3 direction;
        [NonSerialized] public int frames;

        public ForceOverTime Set(int duration, float force, int source, Vector3 direction,
            AnimationCurve animationCurve = null)
        {
            this.animationCurve = animationCurve;
            this.duration = duration;
            this.force = force;
            this.direction = direction;
            this.source = source;
            return this;
        }

        public ForceOverTime()
        {
        }

        public void OnCreateIPoolable()
        {
        }

        public void OnEnableIPoolable()
        {
            frames = 0;
        }

        public void OnDisableIPoolable()
        {
            animationCurve = null;
        }
    }

    public class ForceOverTimeCache : ILazySingleton<ForceOverTimeCache>
    {
        public Pool<ForceOverTime> Cache = new Pool<ForceOverTime>();
    }
}