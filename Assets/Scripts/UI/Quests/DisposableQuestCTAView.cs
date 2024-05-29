using System;
using UnityEngine;
using UnityEngine.UI;

public class DisposableQuestCTAView : ADisposableView
{
    [SerializeField] private Image CTAImage;

    private Transform anchor;
    private Vector3 anchorPosition;

    public void Show(Transform parent, Image image)
    {
        CTAImage.sprite = image.sprite;
        CTAImage.color = image.color;
        anchor = parent;
    }

    public override void Dispose(Action callback, float delay)
    {
        throw new NotImplementedException();
    }

    public override void ResetView()
    {
        throw new NotImplementedException();
    }
    private void LateUpdate()
    {
        if (anchor != null) {
            anchorPosition = anchor.transform.position;
        }
        OverlayCanvas.Instance.AnchorWorldPos(transform as RectTransform, anchorPosition);
    }
}
