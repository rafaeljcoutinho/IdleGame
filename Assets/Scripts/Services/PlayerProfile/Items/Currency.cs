using UnityEngine;

[CreateAssetMenu(fileName ="Currency", menuName = "Item/Currency")]
public class Currency : Item {
    public override bool ShowInInventory => true;
}
