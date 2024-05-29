using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BonfireNodeData", menuName = "Node/BonfireNodeData")]
public class BonfireNodeData : NodeData
{
    public List<ItemWithQuantity> cost;
    public List<ItemWithQuantity> reward;
}