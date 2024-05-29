using UnityEngine;

[CreateAssetMenu(fileName = "dotAuraEffect", menuName = "Item/EquipmentEffect/AuraDOT")]
public class DotAuraParams : EquipmentBehaviourParams
{
    public float Range;
    public int Damage;
    public float FreqHz;
}
