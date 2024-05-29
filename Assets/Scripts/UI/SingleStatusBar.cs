using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SingleStatusBar : MonoBehaviour
{
    [SerializeField] private Image realHealthBar;
    [SerializeField] private Image delayedHealthbar;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private DisplayOptions defaultOptions;
    [SerializeField] private TextMeshProUGUI text;

    private Tween anim;
    private Tween animInner;
    private Transform anchor;
    private float width => (transform as RectTransform).rect.size.x;
    RectTransform realBarRt => realHealthBar.transform as RectTransform;
    RectTransform delayedBarRt => delayedHealthbar.transform as RectTransform;

    private DisplayOptions displayOptions;

    [Serializable]
    public class DisplayOptions
    {
        public Color bleedColor;
        public Color healthColor;
        public Color regenColor;
        public Vector2 Offset;
    }

    private void LateUpdate()
    {
        if (anchor != null)
        {
            OverlayCanvas.Instance.AnchorWorldTransform(transform as RectTransform, anchor);
            (transform as RectTransform).anchoredPosition += displayOptions.Offset * (transform as RectTransform).rect.size;
        }
    }

    public void Reset(float v, string text, Transform anchor, DisplayOptions displayOptions = null)
    {
        this.text.text = text;
        anim?.Kill();
        animInner?.Kill();
        this.anchor = anchor;
        if (anchor != null)
            OverlayCanvas.Instance.AnchorWorldTransform(transform as RectTransform, anchor);
        if (canvasGroup != null)
            canvasGroup.alpha = 1;
        var newWidth = Mathf.Lerp(0, width, v);
        realBarRt.SetSizeWithCurrentAnchors(0, newWidth);
        delayedBarRt.SetSizeWithCurrentAnchors(0, newWidth);
        gameObject.SetActive(true);

        if (displayOptions == null)
        {
            this.displayOptions = defaultOptions;
        }
        else
        {
            this.displayOptions = displayOptions;
        }
        realHealthBar.color = this.displayOptions.healthColor;
    }

    public void Disapear()
    {
        canvasGroup.alpha = 0;
    }

    public void DisposeNoCanvasGroup(Action onDisposeComplete)
    {
        anim?.Kill();
        animInner?.Kill();
        animInner = null;
        anim = null;
        onDisposeComplete?.Invoke();
    }
    
    public void Dispose(Action onDisposeComplete)
    {
        if (animInner != null && animInner.active && animInner.IsPlaying())
        {
            animInner.OnComplete(() =>
            {
                canvasGroup.DOFade(0, .3f).OnComplete(() =>
                {
                    anim?.Kill();
                    animInner = null;
                    gameObject.SetActive(false);
                    anchor = null;
                    onDisposeComplete?.Invoke();
                });
            });
        }
        else
        {
            canvasGroup.DOFade(0, .3f).OnComplete(() =>
            {
                anim?.Kill();
                animInner?.Kill();
                gameObject.SetActive(false);
                anchor = null;
                animInner = null;
                onDisposeComplete?.Invoke();
            });            
        }
    }
    
    public void UpdateValueNormalized(float v, string text, Action callback = null)
    {
        this.text.text = text;
        var newWidth = Mathf.Lerp(0, width, v);
        var currentWidthReal = realBarRt.rect.width;

        if (newWidth < currentWidthReal)
        {
            if (displayOptions != null)
            {
                realHealthBar.color = displayOptions.healthColor;
                delayedHealthbar.color = displayOptions.bleedColor;
            }
            AnimateToLower(v, callback);
        }
        else
        {
            if (displayOptions != null)
            {
                realHealthBar.color = displayOptions.healthColor;
                delayedHealthbar.color = displayOptions.regenColor;
            }
            AnimateToUpper(v, callback);
        }
    }

    private void AnimateToLower(float v, Action callback)
    {
        var newWidth = Mathf.Lerp(0, width, v);
        var currentWidthReal = realBarRt.rect.width;
        var currentWidthDelayed = delayedBarRt.rect.width;

        anim?.Kill();
        animInner?.Kill();

        anim = DOVirtual
            .Float(currentWidthReal, newWidth, .3f, value => { realBarRt.SetSizeWithCurrentAnchors(0, value); })
            .SetEase(Ease.OutExpo);

        animInner = DOVirtual
            .Float(currentWidthDelayed, newWidth, .3f,
                value => { delayedBarRt.SetSizeWithCurrentAnchors(0, value); })
            .SetDelay(.17f).SetEase(Ease.OutExpo).OnComplete(() => callback?.Invoke());
    }
    
    private void AnimateToUpper(float v, Action callback)
    {
        var newWidth = Mathf.Lerp(0, width, v);
        var currentWidthReal = realBarRt.rect.width;
        var currentWidthDelayed = delayedBarRt.rect.width;

        anim?.Kill();
        animInner?.Kill();

        anim = DOVirtual
            .Float(currentWidthDelayed, newWidth, .3f, value => { delayedBarRt.SetSizeWithCurrentAnchors(0, value); })
            .SetEase(Ease.OutExpo);

        animInner = DOVirtual
            .Float(currentWidthReal, newWidth, .3f,
                value => { realBarRt.SetSizeWithCurrentAnchors(0, value); })
            .SetDelay(.17f).SetEase(Ease.OutExpo).OnComplete(() => callback?.Invoke());
    }
}
