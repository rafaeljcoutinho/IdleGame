using UnityEngine;

public interface IConsumable{}

[CreateAssetMenu(fileName ="FastForwardConsumable", menuName = "Item/FastForwardConsumable")]
public class FastForwardConsumable : Item, IConsumable, IInventoryViewable, ISellable
{
    [SerializeField] private int seconds;
    [SerializeField] private ItemWithQuantity cost;
    public int Seconds => seconds;

    public ItemWithQuantity Cost => cost;

    public override bool Accept(ConsumableItemResolver itemResolver)
    {
        return itemResolver.ConsumeItem(this);
    }
}