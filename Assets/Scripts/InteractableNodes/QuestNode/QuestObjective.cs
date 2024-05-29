using System;

[Serializable]
public class QuestObjective
{
    public enum Type
    {
        Have,
        Gather,
        Kill,
    }

    public Type type;
    public Item item;
    public ActionNodeData targetNode;
    public int amount;
}