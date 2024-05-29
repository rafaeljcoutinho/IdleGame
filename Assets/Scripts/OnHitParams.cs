using UnityEngine;

[CreateAssetMenu(fileName = "onHitEffect", menuName = "Item/EquipmentEffect/OnHitEffect")]
public class OnHitParams : EquipmentBehaviourParams
{
    [Header("Deals damage")]
    [SerializeField] public int damage;
    [SerializeField] public float hpDamagePercent;
    [SerializeField] public bool usePercent;
    [SerializeField] public bool doDamage;
    [SerializeField] public float chanceForDamage;
    
    [Header("Heals")]
    [SerializeField] public int flatHeal;
    [SerializeField] public float healPercentMaxHp;
    [SerializeField] public bool usePercentHp;
    [SerializeField] public bool doHeal;
    [SerializeField] public float chanceForHeal;
    
    [Header("StatusEffects")]
    [SerializeField] public bool applyPoison;
    [SerializeField] public float applyPoisonChance;
    [SerializeField] public bool doStatusEffecs;
}