
using System;
using UnityEngine;

[CreateAssetMenu(fileName ="FoodItem", menuName = "Item/EquipFood")]
public class EquipableFood : Equipment, IConsumable, IInventoryViewable
{
    [SerializeField] private HealingAttributes healing;
    public HealingAttributes HealingAttributes => healing;

    public override bool Accept(ConsumableItemResolver itemResolver)
    {
        return itemResolver.ConsumeItem(this);
    }
}

[Serializable]
public class HealingAttributes
{
    public int HpRecovered;
    public float CooldownSeconds;
}