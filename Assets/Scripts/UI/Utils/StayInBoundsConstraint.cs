using UnityEngine;

[ExecuteInEditMode]
public class StayInBoundsConstraint : MonoBehaviour
{
    [SerializeField] private RectTransform container;

    private RectTransform _rectTransform;

    private RectTransform rectTransform =>
        _rectTransform == null ? _rectTransform = GetComponent<RectTransform>() : _rectTransform;

    private Vector3[] containerWorldCornersBuffer = new Vector3[4];
    private Vector3[] selfWorldCornersBuffer = new Vector3[4];

    private Vector3 ContainerBL => containerWorldCornersBuffer[0];
    private Vector3 ContainerTR => containerWorldCornersBuffer[2];
    private Vector3 SelfBL => selfWorldCornersBuffer[0];
    private Vector3 SelfTR => selfWorldCornersBuffer[2];

    private void LateUpdate()
    {
        StayWithinBounds();
    }

    private void StayWithinBounds()
    {
        if (container == null) return;

        rectTransform.GetWorldCorners(selfWorldCornersBuffer);
        container.GetWorldCorners(containerWorldCornersBuffer);

        var dist = SelfBL - ContainerBL;
        var dotX = Vector3.Dot(container.right, dist);
        var dotY = Vector3.Dot(container.up, dist);

        if (dotX < 0) rectTransform.Translate(container.right.normalized * -dotX, Space.World);
        if (dotY < 0) rectTransform.Translate(container.up.normalized * -dotY, Space.World);

        dist = SelfTR - ContainerTR;
        dotX = Vector3.Dot(container.right, dist);
        dotY = Vector3.Dot(container.up, dist);

        if (dotX > 0) rectTransform.Translate(container.right.normalized * -dotX, Space.World);
        if (dotY > 0) rectTransform.Translate(container.up.normalized * -dotY, Space.World);
    }
}