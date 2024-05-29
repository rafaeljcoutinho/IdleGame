using UnityEngine;

[CreateAssetMenu(fileName ="Equipment", menuName = "Item/Equipment")]
public class Equipment : Item, IEquipmentModifier, IInventoryViewable, ISellable
{
    public enum EquipSlot
    {
        MainHand,
        ToolBeltAxe,
        ToolBeltFishingRod,
        ToolBeltPickaxe,
        Food_1,
        Food_2,
        Food_3,
        Food_4,
        Cape,
        Gloves,
        Helm,
        Body,
        Legs,
        Boots,
        Necklace,
        Ring,
        Bracelet,
        OffHand,
    }
    
    [SerializeField] private Requirement requirements;
    [SerializeField] private ItemWithQuantity cost;
    [SerializeField] private EquipmentBehaviour itemPrefab;
    [SerializeField] private EquipSlot equipSlot;
    [SerializeField] private ActionModifiers modifiers;
    [SerializeField] private SkillData.Type mainType;
    [SerializeField] private EquipmentBehaviourParams behaviourParams;

    public Requirement Requirements => requirements;
    public EquipmentBehaviour ItemPrefab => itemPrefab;
    public EquipSlot Slot => equipSlot;
    public ActionModifiers Modifiers => modifiers;
    public SkillData.Type MainType => mainType;
    public ItemWithQuantity Cost => cost;
    public EquipmentBehaviourParams BehaviourParams => behaviourParams;
}
