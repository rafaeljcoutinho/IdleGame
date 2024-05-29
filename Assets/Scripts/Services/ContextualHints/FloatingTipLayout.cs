using UnityEngine;

public interface IFloatingTipLayoutData
{
    ContextualHintService.LayoutType Type { get; }
}

public abstract class FloatingTipLayout : MonoBehaviour
{
    public abstract void Setup(IFloatingTipLayoutData data);
}