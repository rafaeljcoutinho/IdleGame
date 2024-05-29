using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Cooking Recipe", menuName = "Node/Cooking Recipe Data")]
public class CookingRecipe : NodeData
{
    public int hp;
    public float failChance = .1f;
    public List<ItemWithQuantity> Inputs;
    public List<ItemWithQuantity> Output;
}