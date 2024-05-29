using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DroptableDatabase", menuName = "Droptable/Database")]
public class DropTableDatabase : ScriptableObject
{
    [SerializeField] private List<Droptable> droptables;
    private Dictionary<Guid, Droptable> droptablesDictionary;

    public void Start()
    {
        droptablesDictionary = new Dictionary<Guid, Droptable>();
        foreach (var droptable in droptables)
        {
            droptablesDictionary.Add(Guid.Parse(droptable.id), droptable);
        }
    }

    public Droptable GetDroptable(Guid guid)
    {
        if (!droptablesDictionary.ContainsKey(guid))
            return null;
        return droptablesDictionary[guid];
    }
}