using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDrops", menuName = "Item/MonsterDrops")]
public class MonsterDrops : Item, IInventoryViewable, ISellable
{
    [SerializeField] private ItemWithQuantity cost;
    public ItemWithQuantity Cost => cost;
}