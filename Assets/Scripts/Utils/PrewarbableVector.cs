using System;

public interface IPrewarmable
{
    void Reset();
}

public class StaticSizeVectorPrecached<T> : StaticSizeVector<T> where T : IPrewarmable
{
    public StaticSizeVectorPrecached(int size) : base(size) {}

    public void PrewarmSlice()
    {
        for (var i = 0; i < slice.Length; i++)
        {
            slice[i] = Activator.CreateInstance<T>();
        }
    }

    public T PushBackResetCached()
    {
        count++;
        slice[count-1].Reset();
        return slice[count-1];
    }
}

public class StaticSizeVector<T>
{
    protected T[] slice;
    protected int count;

    public StaticSizeVector(int size)
    {
        slice = new T[size];
    }

    public void Clear()
    {
        count = 0;
    }

    public void PushBack(T obj)
    {
        slice[count] = obj;
        count++;
    }

    public int Capacity => slice.Length;
    public int Count => count;
    public T this[int i] => slice[i];
}