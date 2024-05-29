using System;
using UnityEngine;

public abstract class ADisposableView : MonoBehaviour
{
    public abstract void Dispose(Action callback, float delay);
    public abstract void ResetView();
}

public class DisposableViewPool : MonoBehaviour
{
    [SerializeField] private ADisposableView damageNumberPrefab;
    [SerializeField] private Transform container;
    private ObjectPool<ADisposableView> damageNumberPool;

    private void Start()
    {
        damageNumberPool = new ObjectPool<ADisposableView>(container, damageNumberPrefab);
        damageNumberPool.Prewarm(10);
    }

    public T Get<T>() where T : ADisposableView
    {
        var view = damageNumberPool.Pop();
        view.ResetView();
        return view as T;
    }
    
    public void Return(ADisposableView view, float delay)
    {
        view.Dispose(() => damageNumberPool.Push(view), delay);
    }
}