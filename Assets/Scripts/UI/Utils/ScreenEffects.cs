using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffects : MonoBehaviour
{
    [SerializeField] private Image image;

    public void FadeOut(bool instant = true)
    {
        var imageColor = image.color;
        imageColor.a = 1;
        image.color = imageColor;
    }
    
    public void FadeOut(Action onComplete)
    {
        image.DOFade(1, .2f).OnComplete(() => onComplete?.Invoke());
    }

    public void FadeIn()
    {
        image.DOFade(0, .2f);
    }
}