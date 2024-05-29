using System;
using UnityEngine;

[CreateAssetMenu(fileName ="NodeData", menuName = "Node/MapNode")]
public class MapNodeConfig : NodeData
{
    [SerializeField] private string sceneName;
    [SerializeField] private QuestObjective objective;
    [SerializeField] private MapNodeConfig nextMap;
    
    public Guid Id => Guid.Parse(id);
    public string SceneName => sceneName;
    public Requirement Requirement => requirement;
    public QuestObjective Objective => objective;
    public MapNodeConfig NextMap => nextMap;
}