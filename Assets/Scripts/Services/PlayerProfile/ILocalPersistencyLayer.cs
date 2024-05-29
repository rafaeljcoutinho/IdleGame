using System;

public interface ILocalPersistencyLayer
{
    public void Load<T>(Action<T, bool> onLoad, T _default) where T : class;
    public void Save<T>(T blob, Action<bool> saved);
}