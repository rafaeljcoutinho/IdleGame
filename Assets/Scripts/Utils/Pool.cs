using System;
using System.Collections.Generic;


public interface IPoolable
{
    void OnCreateIPoolable();
    void OnEnableIPoolable();
    void OnDisableIPoolable();
} 

public class Pool<T> where T : IPoolable
{
    private List<T> available;
    private List<T> all;
    private HashSet<T> inUse;
    private Dictionary<T, int> indexInAllArray;

    public HashSet<T> InUse => inUse;

    public Pool(int defaultSize = 2)
    {
        available = new List<T>();
        all = new List<T>();
        inUse = new HashSet<T>();
        indexInAllArray = new Dictionary<T, int>();
        AddToPool(defaultSize);
    }

    protected virtual T Instantiate()
    {
        return Activator.CreateInstance<T>();
    }
    
    void AddToPool(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var instance = Instantiate();
            var j = all.Count;
            all.Add(instance);
            available.Add(instance);
            indexInAllArray.Add(instance, j);
            instance.OnCreateIPoolable();
        }
    }

    public void Return(T toReturn)
    {
        var index = indexInAllArray[toReturn];
        var instance = all[index];
        instance.OnDisableIPoolable();
        inUse.Remove(toReturn);
    }
    
    public T GetOrCreate()
    {
        if (available.Count == 0) AddToPool(all.Count);
        var instance = available[available.Count-1];
        available.RemoveAt(available.Count-1);
        instance.OnEnableIPoolable();
        inUse.Add(instance);
        return instance;
    }
}
