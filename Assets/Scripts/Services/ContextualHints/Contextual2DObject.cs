using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Contextual2DObject : MonoBehaviour
{
        public enum AnchorPosition
    {
        Auto,
        Top,
        Bottom,
        Center,
    }

    [SerializeField] private Image background;
    [SerializeField] private RectTransform root;
    [SerializeField] private RectTransform container;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private RectTransform bubble;
    [SerializeField] private Image bubbleBg;
    [SerializeField] private Image topArrow;
    [SerializeField] private Image bottomArrow;
    [SerializeField] private RectTransform arrowContainer;
    [SerializeField] private RectTransform worldSpaceAdapter;

    private RectTransform selfRT;

    private Vector3[] targetWorldCornersBuffer = new Vector3[4];
    private Vector3[] bubbleWorldCornersBuffer = new Vector3[4];
    private Vector3[] containerWorldCornersBuffer = new Vector3[4];
    private Vector3[] arrowWorldCornersBuffer = new Vector3[4];
    private Vector3[] arrowContainerCornersBuffer = new Vector3[4];

    private Vector3 TargetBL => targetWorldCornersBuffer[0];
    private Vector3 TargetTR => targetWorldCornersBuffer[2];
    private Vector3 BubbleBL => bubbleWorldCornersBuffer[0];
    private Vector3 BubbleTR => bubbleWorldCornersBuffer[2];
    private Vector3 ContainerBL => containerWorldCornersBuffer[0];
    private Vector3 ContainerTR => containerWorldCornersBuffer[2];
    private Vector3 ArrowBL => arrowWorldCornersBuffer[0];
    private Vector3 ArrowTR => arrowWorldCornersBuffer[2];
    private Vector3 ArrowContainerBL => arrowContainerCornersBuffer[0];
    private Vector3 ArrowContainerTR => arrowContainerCornersBuffer[2];

    private float DisplayOffset => displayOptions.offset * BubbleRect.lossyScale.y * topArrow.rectTransform.rect.height;
    private float dismissTimer = 0;
    private bool isShowing = false;
    public bool IsShowing => isShowing;
    
    private ContextualHintService.HintDisplayOptions displayOptions;
    public RectTransform TargetRectTransform { get; set; }
    public Transform TargetTransform { get; set; }
    public RectTransform VisibilityContainer { get; set; }
    
    private Sequence fadeAnim;
    public RectTransform Root => root;
    public RectTransform BubbleRect => bubble;
    public ContextualHintService.HintDisplayOptions DisplayOptions => displayOptions;

    private void Start()
    {
        var service = Services.Container.Resolve<ContextualHintService>();
        service?.RegisterOverlay(this);
        
        selfRT = transform as RectTransform;
    }

    public void SetDisplayOptions(ContextualHintService.HintDisplayOptions displayOptions)
    {
        this.displayOptions = displayOptions;
        return;
        topArrow.color = displayOptions.isDarkMode ? Color.black : Color.white;
        bottomArrow.color = displayOptions.isDarkMode ? Color.black : Color.white;
        bubbleBg.color = displayOptions.isDarkMode ? Color.black : Color.white;
    }

    public IEnumerator ShowDelayed(float delay, Action onShow)
    {
        yield return new WaitForSeconds(delay);
        onShow?.Invoke();
        Show();
    }

    public void Show()
    {
        isShowing = true;
        StopAllCoroutines();
        fadeAnim?.Kill();
        fadeAnim = DOTween.Sequence();
        fadeAnim.Join(canvasGroup.DOFade(1, .3f));

        var backgroundColor = background.color;
        backgroundColor.a = 0;
        background.color = backgroundColor;
        
        if (displayOptions.fadeBackground) 
            fadeAnim.Join(background.DOFade(.7f, .2f));

        fadeAnim.Play();

        if (displayOptions.autoHideSeconds != 0)
            dismissTimer = Time.time + displayOptions.autoHideSeconds;
    }

    public IEnumerator ShowWhenVisible(float delay, Action onShow)
    {
        float elapsedTime = 0;
        while (elapsedTime <= delay)
        {
            if (TargetRectTransform == null) yield break;
            var canShow = IsVisible(TargetRectTransform) && !Input.GetMouseButtonDown(0);
            elapsedTime = canShow ? elapsedTime + Time.deltaTime : 0;
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        onShow?.Invoke();
        Show();
    }

    bool IsVisible(RectTransform rectTransform)
    {
        var visibilityContainer = VisibilityContainer == null ? container : VisibilityContainer;
        return Utility.IsRectTransformInsideOther(visibilityContainer, rectTransform);
    }

    public void Hide(bool instant = false)
    {
        isShowing = false;
        TargetRectTransform = null;
        VisibilityContainer = null;
        StopAllCoroutines();
        fadeAnim?.Kill();
        if (instant)
        {
            canvasGroup.alpha = 0;
            var tempColor = background.color;
            tempColor.a = 0;
            background.color = tempColor;
        }
        else
        {
            fadeAnim = DOTween.Sequence();
            fadeAnim
                .Join(canvasGroup.DOFade(0, .2f))
                .Join(background.DOFade(0, .2f));
        }
        dismissTimer = 0f;
    }

    private void LateUpdate()
    {
        if (TargetTransform != null)
        {
            OverlayCanvas.Instance.AnchorWorldTransform(worldSpaceAdapter, TargetTransform);
            TargetRectTransform = worldSpaceAdapter;
        }
        if (TargetRectTransform == null)
        {
            if (isShowing) Hide();
            return;
        }

        if (displayOptions?.autoHideSeconds > 0 && Time.time > dismissTimer)
        {
            if (isShowing)
                Hide();
            return;
        }
        TargetRectTransform.GetWorldCorners(targetWorldCornersBuffer);
        BubbleRect.GetWorldCorners(bubbleWorldCornersBuffer);
        container.GetWorldCorners(containerWorldCornersBuffer);
        topArrow.rectTransform.GetWorldCorners(arrowWorldCornersBuffer);
        arrowContainer.GetWorldCorners(arrowContainerCornersBuffer);

        var anchorPosition = displayOptions.anchorPosition == AnchorPosition.Auto
            ? AutoChooseAnchor()
            : displayOptions.anchorPosition;
        
        var deltaY = 0f;
        switch (anchorPosition)
        {
            case AnchorPosition.Top:
                deltaY = SetAnchoredOnTop();
                break;
            case AnchorPosition.Bottom:
                deltaY = SetAnchoredOnBottom();
                break;
            case AnchorPosition.Center:
                deltaY = SetAnchoredOnCenter(TargetRectTransform);
                break;
        }

        var deltaX = UpdateBubblePositionX(TargetRectTransform);
        Utility.StayWithinXBounds(deltaX, BubbleBL, ContainerBL, BubbleTR, ContainerTR, BubbleRect, container);
        UpdateArrowPositionX(TargetRectTransform);
        
        BubbleRect.Translate(bottomArrow.rectTransform.rect.height/40f * Vector3.up * Mathf.Sin(Time.time*2), Space.World);
        
        if (!isShowing) return;
        if (!IsVisible(TargetRectTransform))
        {
            //Hide();
        }
    }

    private float UpdateBubblePositionX(RectTransform rectTransform)
    {
        var dif = rectTransform.position - BubbleRect.position;
        var dot = Vector3.Dot(container.right, dif);
        BubbleRect.Translate(container.right.normalized * dot, Space.World);
        return dot;
    }

    private AnchorPosition AutoChooseAnchor()
    {
        var containerCenter = (ContainerTR + ContainerBL) / 2;
        var itemCenter = (TargetTR + TargetBL) / 2;

        if (itemCenter.y > containerCenter.y) return AnchorPosition.Bottom;
        return AnchorPosition.Top;
    }

    private void EnableTopArrow(bool enableTopArrow)
    {
        bottomArrow.gameObject.SetActive(!enableTopArrow);
        topArrow.gameObject.SetActive(enableTopArrow);
    }

    void UpdateArrowPositionX(RectTransform item)
    {
        var dif = item.position - topArrow.rectTransform.position;
        var dot = Vector3.Dot(container.right, dif);

        topArrow.rectTransform.Translate(container.right.normalized * dot, Space.World);
        bottomArrow.rectTransform.Translate(container.right.normalized * dot, Space.World);
        Utility.StayWithinXBounds(dot, ArrowBL, ArrowContainerBL, ArrowTR, ArrowContainerTR, topArrow.rectTransform, arrowContainer);
        Utility.StayWithinXBounds(dot, ArrowBL, ArrowContainerBL, ArrowTR, ArrowContainerTR, bottomArrow.rectTransform,
            arrowContainer);
    }

    private float SetAnchoredOnBottom()
    {
        var dif = TargetBL - BubbleTR;
        var dot = Vector3.Dot(container.up, dif) - DisplayOffset;
        BubbleRect.Translate(container.up.normalized * dot, Space.World);
        EnableTopArrow(true);
        return dot;
    }

    private float SetAnchoredOnCenter(RectTransform rectTransform)
    {
        var dif = rectTransform.position - BubbleRect.position;
        var dot = Vector3.Dot(container.up, dif) + DisplayOffset;
        BubbleRect.Translate(container.up.normalized * dot, Space.World);
        EnableTopArrow(false);
        return dot;
    }

    private float SetAnchoredOnTop()
    {
        var dif = TargetTR - BubbleBL;
        var dot = Vector3.Dot(container.up, dif) + DisplayOffset;
        BubbleRect.Translate(container.up.normalized * dot, Space.World);
        EnableTopArrow(false);
        return dot;
    }

    IEnumerator ListenForInteraction(Action callback)
    {
        while (Time.time < dismissTimer)
        {
            yield return null;
        }

        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }

        callback?.Invoke();
    }

    public class Utility
    {
        public static bool IsRectTransformInsideOther(RectTransform container, RectTransform target)
        {
            var objectCorners = new Vector3[4];
            var worldCornersContainer = new Vector3[4];
    
            container.GetWorldCorners(worldCornersContainer);
            target.GetWorldCorners(objectCorners);
        
            var dist = objectCorners[0] - worldCornersContainer[0];
            var dotX = Vector3.Dot(container.right, dist);
            var dotY = Vector3.Dot(container.up, dist);

            if (dotX < 0 || dotY < 0)
            {
                return false;
            }

            dist = objectCorners[2] - worldCornersContainer[2];
            dotX = Vector3.Dot(container.right, dist);
            dotY = Vector3.Dot(container.up, dist);

            return dotX < 0 && dotY < 0;
        }

        public static (float, float) StayWithinBounds(float offsetX, float offsetY,
            Vector3 originBL, Vector3 targetBL,
            Vector3 originTR, Vector3 targetTR,
            RectTransform originTransform, RectTransform targetTransform)
        {
            return (
                StayWithinXBounds(offsetX, originBL, targetBL, originTR, targetTR, originTransform, targetTransform),
                StayWithinYBounds(offsetY, originBL, targetBL, originTR, targetTR, originTransform, targetTransform));
        }

        public static float StayWithinXBounds(float offset,
            Vector3 originBL, Vector3 targetBL,
            Vector3 originTR, Vector3 targetTR,
            RectTransform originTransform, RectTransform targetTransform)
        {
            var ans = 0f;
            var dist = originBL - targetBL;
            var dot = Vector3.Dot(targetTransform.right, dist) + offset;

            if (dot < 0)
            {
                originTransform.Translate(targetTransform.right.normalized * -dot, Space.World);
                ans += dot;
            }

            dist = originTR - targetTR;
            dot = Vector3.Dot(targetTransform.right, dist) + offset;

            if (dot > 0)
            {
                originTransform.Translate(targetTransform.right.normalized * -dot, Space.World);
                ans += dot;
            }

            return ans;
        }

        public static float StayWithinYBounds(float offset,
            Vector3 originBL, Vector3 targetBL,
            Vector3 originTR, Vector3 targetTR,
            RectTransform originTransform, RectTransform targetTransform)
        {
            var ans = 0f;
            var dist = originBL - targetBL;
            var dot = Vector3.Dot(targetTransform.up, dist) + offset;

            if (dot < 0)
            {
                originTransform.Translate(targetTransform.up.normalized * -dot, Space.World);
                ans += dot;
            }

            dist = originTR - targetTR;
            dot = Vector3.Dot(targetTransform.up, dist) + offset;

            if (dot > 0)
            {
                originTransform.Translate(targetTransform.up.normalized * -dot, Space.World);
                ans += dot;
            }

            return ans;
        }
    }
}