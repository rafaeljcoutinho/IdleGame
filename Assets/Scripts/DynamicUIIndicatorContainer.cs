using UnityEngine;

public class DynamicUIIndicatorContainer : MonoBehaviour
{
    public T InstantiateUI<T>(T prefab) where T : Component
    {
        return Instantiate(prefab, transform);
    }
}