using System;
using UnityEngine;

public class QuestDatabaseService
{
    private QuestsDatabase questDatabase;
    public QuestsDatabase QuestDatabase => questDatabase;

    public void Load(Action<bool> callback)
    {
        var loadOp = Resources.LoadAsync<QuestsDatabase>("QuestDatabase");
        loadOp.completed += operation =>
        {
            if (operation.isDone)
            {
                questDatabase = loadOp.asset as QuestsDatabase;
            }
            questDatabase.Start();
            callback?.Invoke(true);
        };
    }
}