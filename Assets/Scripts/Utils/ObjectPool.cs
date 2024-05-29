using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectPool<T> where T : Component
{
    private T prefab;
    private Transform root;
    public Transform Root => root;
    private Queue<T> freeObjects;
    private HashSet<T> usedObjects;

    public HashSet<T> UsedObjects => usedObjects;

    public ObjectPool(Transform t, T prefab)
    {
        root = t;
        this.prefab = prefab;
        freeObjects = new Queue<T>(10);
        usedObjects = new HashSet<T>(10);
    }
    
    public ObjectPool(Transform t, GameObject gameObject)
    {
        root = t;
        prefab = gameObject.GetComponent<T>();
        freeObjects = new Queue<T>(10);
        usedObjects = new HashSet<T>(10);
    }

    public void Prewarm(int count)
    {
        Instantiate(count);
    }
    
    private void Instantiate(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var instance = GameObject.Instantiate(prefab, root);
            freeObjects.Enqueue(instance);
        }
    }
    
    public T Pop()
    {
        if (freeObjects.Count == 0)
            Instantiate(Math.Max(1, root.childCount));

        var obj = freeObjects.Dequeue();
        usedObjects.Add(obj);
        return obj;
    }

    public void Push(T obj)
    {
        usedObjects.Remove(obj);
        freeObjects.Enqueue(obj);
    }
}