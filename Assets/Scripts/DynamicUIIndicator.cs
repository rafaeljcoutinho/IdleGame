using UnityEngine;

public class DynamicUIIndicator : MonoBehaviour
{
    private Vector3 anchorPos;
    private Vector3 offset;
    private Transform anchor;

    private bool showing = false;

    public void Show(Transform target, Vector3 offset)
    {
        showing = true;
        this.anchor = target;
        this.offset = offset;
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        if (!showing)
        {
            return;
        }
        if (anchor != null) {
            anchorPos = anchor.transform.position;
        }
        OverlayCanvas.Instance.AnchorWorldPos(transform as RectTransform, anchorPos);
        (transform as RectTransform).anchoredPosition += offset * (transform as RectTransform).rect.size;
    }
}
