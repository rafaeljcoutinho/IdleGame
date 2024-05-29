using UnityEngine;

[CreateAssetMenu(fileName ="ActionNodeData", menuName = "Node/ActionNodeData")]
public class ActionNodeData : NodeData
{
    public int hp;
    public int flatResistance;
    public float percentResistence;
    public float dodge;
    public float respawnCooldown;
    public Droptable droptable;
    public float chanceToAutoRespawn;
    public int level;
    public GameObject prefab;
}