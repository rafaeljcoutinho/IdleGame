using UnityEngine;

[CreateAssetMenu(menuName = "Game/EnemyArea", fileName = "enemyAreaData")]
public class EnemyAreaData : ScriptableObject
{
    public ActionNodeData enemy;
    public int quantity;
    public float timeToKill;
}