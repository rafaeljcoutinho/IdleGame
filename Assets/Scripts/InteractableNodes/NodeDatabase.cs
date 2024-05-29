using System;
using System.Collections.Generic;
using UnityEngine;

public class NodeDatabaseService
{
    private NodeDatabase nodeDatabase;
    public NodeDatabase NodeDatabase => nodeDatabase;

    public void Load(Action<bool> callback)
    {
        var loadOp = Resources.LoadAsync<NodeDatabase>("NodeDatabase");
        loadOp.completed += operation =>
        {
            if (operation.isDone)
            {
                nodeDatabase = loadOp.asset as NodeDatabase;
            }
            nodeDatabase.Start();
            callback?.Invoke(true);
        };
    }
}

[CreateAssetMenu(fileName = "NodeDatabase", menuName = "Node/Database")]
public class NodeDatabase : ScriptableObject
{
    [SerializeField] private List<NodeData> Nodes;
    private Dictionary<Guid, NodeData> nodeDic;

    public List<NodeData> AllNodes => Nodes;

    public void Start()
    {
        nodeDic = new Dictionary<Guid, NodeData>();
        foreach (var item in Nodes)
        {
            nodeDic.Add(Guid.Parse(item.id), item);
        }
    }

    public NodeData GetNode(Guid guid)
    {
        return nodeDic[guid];
    }
}