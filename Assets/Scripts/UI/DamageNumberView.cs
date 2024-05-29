using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageNumberView : ADisposableView
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup cg;

    private Transform anchor;
    private Vector3 offset;
    private Vector3 anchorPos;
    private Sequence popAnimation;

    public class ViewData
    {
        public Color color;
        public string text;
        public float spread = .2f;
    }

    public override void ResetView()
    {
        cg.alpha = .8f;
        transform.localScale = Vector3.one;
        popAnimation?.Kill();
        popAnimation = null;
    }

    private void LateUpdate()
    {
        if (anchor != null) {
            anchorPos = anchor.transform.position;
        }
        OverlayCanvas.Instance.AnchorWorldPos(transform as RectTransform, anchorPos);
        (transform as RectTransform).anchoredPosition += offset * (transform as RectTransform).rect.size;
    }

    public override void Dispose(Action onComplete, float delay)
    {
        Services.Container.Resolve<CoroutineDispatcher>().AfterDelay(delay, () =>
        {
            popAnimation?.Kill();
            popAnimation = null;
            cg.DOFade(0f, .1f).OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        });
    }

    private static float rotatingSpread;
    public void Show(ViewData viewData, Transform anchor, Vector3 offsetParam, float delayDuration = 0f)
    {
        text.text = viewData.text;
        text.color = viewData.color;
        this.anchor = anchor;
        anchorPos = anchor.position;
        offset = offsetParam;
        gameObject.SetActive(true);
        
        Vector3 direction = Vector3.up + ( Vector3.right * rotatingSpread);
        rotatingSpread += .5f;
        if (rotatingSpread > 1)
        {
            rotatingSpread = -1f;
        }
        direction = direction.normalized * viewData.spread;
        popAnimation?.Kill();
        popAnimation = DOTween.Sequence();

        var cached = offset + direction;
        popAnimation.Join(transform.DOPunchScale(Vector3.one * 1.3f, .2f)).Join(cg.DOFade(1, .1f).SetDelay(delayDuration))
            .Join(DOTween.To(() => offset, x => offset = x, cached , .2f).SetEase(Ease.InOutCubic));
    }

    public void ShowSimple(ViewData viewData, Transform anchor, Vector3 offsetParam, float delayDuration = 0f)
    {
        cg.alpha = 0f;
        text.text = viewData.text;
        text.color = viewData.color;
        this.anchor = anchor;
        anchorPos = anchor.position;
        offset = offsetParam;
        gameObject.SetActive(true);
        
        Vector3 direction = Random.insideUnitCircle * viewData.spread;
        popAnimation?.Kill();
        popAnimation = DOTween.Sequence();

        var cached = offset + direction;
        popAnimation.Join(cg.DOFade(.8f, .3f).SetDelay(delayDuration))
            .Join(DOTween.To(() => offset, x => offset = x, cached , .2f).SetEase(Ease.InOutCubic));
    }
}