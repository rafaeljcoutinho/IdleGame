using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkillProgressBar : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private SingleStatusBar statusBarMain;
    [SerializeField] private SingleStatusBar statusBarSecondary;
    [SerializeField] private Image skillImage;
    [SerializeField] private SingleStatusBar.DisplayOptions mainBarDisplayOptions;
    [SerializeField] private SingleStatusBar.DisplayOptions secondBarDisplayOptions;

    private SkillData.Type skillType;
    public SingleStatusBar MainStatusBar => statusBarMain;
    public SingleStatusBar SecondaryStatusBar => statusBarSecondary;
    
    private Transform anchor;
    public Vector2 Offset;
    

    public void SetSkill(SkillData.Type skillType)
    {
        this.skillType = skillType;
    }
    
    public void Disapear()
    {
        cg.alpha = 0;
    }
    public void Reset(float mainBar, float secondBar, Transform anchor, Sprite icon)
    {
        statusBarMain.Reset(mainBar, "", null, mainBarDisplayOptions);
        statusBarSecondary.Reset(secondBar, "", null, secondBarDisplayOptions);
        skillImage.sprite = icon;
        this.anchor = anchor;
        cg.alpha = 1;
    }

    private void LateUpdate()
    {
        if (anchor != null)
        {
            OverlayCanvas.Instance.AnchorWorldTransform(transform as RectTransform, anchor);
            (transform as RectTransform).anchoredPosition += Offset * (transform as RectTransform).rect.size;
        }
    }

    public void Dispose(Action onDispose)
    {
        cg.DOFade(0, .3f).OnComplete(() =>
                {
                    anchor = null;
                    onDispose?.Invoke();
                });
        statusBarMain.DisposeNoCanvasGroup(null);
        statusBarSecondary.DisposeNoCanvasGroup(null);
    }

    public void OnSkillInfoButton()
    {
        OverlayCanvas.Instance.HealthBarManager.OnSkillProgressBarClick(skillType);
    }
} 
