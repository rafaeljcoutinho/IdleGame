using System;
using UnityEngine;

public class Blocker : MonoBehaviour
{
    private Action callback;
    public void Show(Action onClick)
    {
        callback = onClick;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        callback = null;
        gameObject.SetActive(false);
    }

    public void OnClick()
    {
        callback?.Invoke();
    }
}
