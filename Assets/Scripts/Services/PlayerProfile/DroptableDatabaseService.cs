using System;
using UnityEngine;

public class DroptableDatabaseService
{
    private DropTableDatabase dropTableDatabase; 
    public DropTableDatabase DropTableDatabase => dropTableDatabase;

    public void Load(Action<bool> callback)
    {
        var loadOp = Resources.LoadAsync<DropTableDatabase>("DroptableDatabase");
        loadOp.completed += operation =>
        {
            if (operation.isDone)
            {
                dropTableDatabase = loadOp.asset as DropTableDatabase;
            }
            dropTableDatabase.Start();
            callback?.Invoke(true);
        };
    }
}