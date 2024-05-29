using System;
using UnityEngine;

[CreateAssetMenu(fileName ="NodeData", menuName = "Node/Data")]
public class 
    NodeData : ScriptableObject
{
    [ScriptableObjectId] public string id;
    public Guid Uuid => Guid.Parse(id);
    public new string name;
    public Sprite icon;
    public Requirement requirement;
    
    [ContextMenu("ResetGuid")]
    public void ResetGuid()
    {
        id = Guid.NewGuid().ToString();
    }
}