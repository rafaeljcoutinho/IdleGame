using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform knob;
    [SerializeField] private RectTransform outer;
    [SerializeField] private float maxDistance;
    [SerializeField] private Camera cam;

    private Vector3 initialPosition;
    private Vector3 previousMousePosition;

    public Vector3 Direction
    {
        get
        {
            var originalDir = knob.transform.localPosition;
            var dir = new Vector3(originalDir.x, 0, originalDir.y);
            return dir.magnitude < 30
                ? Vector3.zero
                : dir.normalized;
        }
    }

    void SetPosition(PointerEventData eventData)
    {
        knob.transform.position = eventData.position;

        if (knob.transform.localPosition.magnitude > maxDistance)
        {
            knob.transform.localPosition = knob.transform.localPosition.normalized * maxDistance;
        } 
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        SetPosition(eventData);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        knob.transform.localPosition = Vector3.zero;
        outer.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        outer.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        outer.gameObject.SetActive(true);
        SetPosition(eventData);
    }
    
    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        var localPoint = Vector2.zero;
        var baseRect = transform as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, cam, out localPoint))
        {
            var pivotOffset = baseRect.pivot * baseRect.sizeDelta;
            return localPoint - (outer.anchorMax * baseRect.sizeDelta) + pivotOffset;
        }
        return Vector2.zero;
    }
}