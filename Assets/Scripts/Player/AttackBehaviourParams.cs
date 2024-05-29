using System.Collections.Generic;
using UnityEngine;

public class AttackBehaviourParams : MonoBehaviour
{
    [SerializeField] private AnimatorWrapper animator;
    [SerializeField] private BoxCollider swordHitRange;
    [SerializeField] private ParticleSystem swordSlash;
    [SerializeField] private ParticleSystem hitFx;
    [SerializeField] private AnimationEventDispatcher eventDispatcher;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private List<Collider> ignoreColliders;

    public AnimatorWrapper Animator => animator;
    public BoxCollider SwordHitRange => swordHitRange;
    public ParticleSystem SwordSlash => swordSlash;
    public ParticleSystem HitFx => hitFx;
    public AnimationEventDispatcher EventDispatcher => eventDispatcher;
    public LayerMask AttackableLayer => attackableLayer;
    public List<Collider> IgnoreColliders => ignoreColliders;
}