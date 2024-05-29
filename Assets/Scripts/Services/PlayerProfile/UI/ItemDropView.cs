using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDropView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI text;

    public ViewData viewData;
    private Sequence animationTween;
    
    public bool IsShowing { get; private set; }
    
    public struct ViewData
    {
        public string text;
        public long amount;
        public Item item;

        public float expiresAt;
        public int index;
    }

    public void ResetData()
    {
        animationTween?.Kill();
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
        transform.SetAsLastSibling();
    }
    
    public void Show(Vector3 from, Vector3 to, Action onComplete, bool animateOut = true)
    {
        IsShowing = true;
        transform.position = from;
        canvasGroup.alpha = 0;
        gameObject.SetActive(true);

        animationTween?.Kill();
        animationTween = DOTween.Sequence();
        if (animateOut)
        {
            animationTween.Append(canvasGroup.DOFade(1, .2f))
                .Join(transform.DOMove(to, 1.5f).SetDelay(.6f).SetEase(Ease.OutExpo))
                .Join(canvasGroup.DOFade(0, .3f)).OnComplete(() => onComplete?.Invoke());   
        }
        else
        {
            animationTween.Join(canvasGroup.DOFade(1, .2f))
                .OnComplete(() => onComplete?.Invoke());   
        }
    }

    public void Hide(Action onComplete)
    {
        IsShowing = false;
        animationTween?.Kill();
        animationTween = DOTween.Sequence();
        animationTween.Join(canvasGroup.DOFade(0, .3f)).OnComplete(() => onComplete?.Invoke());   
    }

    public void ApplyViewData()
    {
        thumbnail.sprite = viewData.item.SmallThumbnail;
        text.text = viewData.text;
        if (viewData.amount > 0)
        {
            var itemNames = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.ItemNameRarityColorPairs;
            text.text = string.Format("+{0} {1}", SkillExperienceTable.Format(viewData.amount), viewData.text);
            text.color = itemNames[(int)viewData.item.rarity].color;
        }
    }
}