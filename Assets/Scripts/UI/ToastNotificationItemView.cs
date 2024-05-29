using System;
using DG.Tweening;
using UnityEngine;

public class ToastNotificationItemView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private FloatingTipLayout layout;
    private Sequence animationTween;

    public void Reset()
    {
        animationTween?.Kill();
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
        transform.SetAsLastSibling();
    }

    public void UpdateLayoutData(IFloatingTipLayoutData data)
    {
        layout.Setup(data);
    }
    
    public void Show(RectTransform spawnPoint, RectTransform target, Action onComplete)
    {
        var from = spawnPoint.transform.position;
        var to = target.transform.position;

        transform.position = from;
        gameObject.SetActive(true);

        animationTween?.Kill();
        animationTween = DOTween.Sequence();
        animationTween.Append(canvasGroup.DOFade(1, .3f)).Join(transform.DOMove(to, .5f).SetEase(Ease.OutExpo))
            .Append(canvasGroup.DOFade(0, .3f).SetDelay(.5f)).OnComplete(() => onComplete?.Invoke());
    }
}