using System;
using UnityEngine;

public class ILazySingleton<T>
{
    private static T _instance;
    public static T Instance => _instance ??= Activator.CreateInstance<T>();
}

public class SingletonBehaviour<T> : MonoBehaviour where T: SingletonBehaviour<T>
{
    public static T Instance { get; protected set; }
 
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            throw new System.Exception("An instance of this singleton already exists.");
        }
        else
        {
            Instance = (T)this;
        }
    }
}
