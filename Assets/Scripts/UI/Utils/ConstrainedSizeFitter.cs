using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(ConstrainedSizeFitter))]
[CanEditMultipleObjects]
public class ConstrainedSizeFitterEditor : Editor
{
}
#endif

public class ConstrainedSizeFitter : ContentSizeFitter
{
    [SerializeField] private RectTransform containedWithin;
    
    [System.NonSerialized] private RectTransform m_Rect;
    protected RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }
    
    [System.NonSerialized] private HorizontalOrVerticalLayoutGroup m_parentLayoutGroup;
    private HorizontalOrVerticalLayoutGroup parentLayoutGroup
    {
        get
        {
            if (m_parentLayoutGroup == null)
                m_parentLayoutGroup = transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
            return m_parentLayoutGroup;
        }
    }
    
    private DrivenRectTransformTracker m_Tracker;
    
    protected override void OnDisable()
    {
        m_Tracker.Clear();
        base.OnDisable();
    }

    public void SetConstrainedWithin(RectTransform constrainedWithin)
    {
        this.containedWithin = constrainedWithin;
        SetDirty();
    }
    
    private void HandleSelfFittingAlongAxis(int axis)
    {
        FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
        if (fitting == FitMode.Unconstrained)
        {
            // Keep a reference to the tracked transform, but don't control its properties:
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
            return;
        }

        m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

        var desiredSize = float.MaxValue;
        if (containedWithin != null)
        {
            desiredSize = axis == 0 ? containedWithin.rect.width : containedWithin.rect.height; 
        }

        if (parentLayoutGroup != null && axis == 0)
        {
            desiredSize -= parentLayoutGroup.padding.left + parentLayoutGroup.padding.right;
        }
        if (parentLayoutGroup != null && axis == 1)
        {
            desiredSize -= parentLayoutGroup.padding.top + parentLayoutGroup.padding.bottom;
        }

        desiredSize = Mathf.Max(0, desiredSize);

        if (fitting == FitMode.MinSize)
             desiredSize = Mathf.Min(desiredSize, LayoutUtility.GetMinSize(m_Rect, axis));
        else
            desiredSize = Mathf.Min(desiredSize, LayoutUtility.GetPreferredSize(m_Rect, axis));

        rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, desiredSize);
    }

    public override void SetLayoutHorizontal()
    {
        m_Tracker.Clear();
        HandleSelfFittingAlongAxis(0);
    }
    
    public override void SetLayoutVertical()
    {
        m_Tracker.Clear();
        HandleSelfFittingAlongAxis(1);
    }
}
