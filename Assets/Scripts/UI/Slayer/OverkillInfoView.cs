using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class OverkillInfoView : MonoBehaviour
{
    [SerializeField] private ChestInfoView ksInfoView;
    [SerializeField] private TextMeshProUGUI overkillValue;
    [SerializeField] private TextMeshProUGUI bonusDropsLabel;
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TextMeshProUGUI timeLeftLabel;

    private Guid trackedOverkill;
    private bool isShowing;
    private Tween fadeAnim;

    public bool IsShowing => isShowing;

    private void Start()
    {
//        Services.Container.Resolve<OverkillService>().OnNodeDeath += Track;
    }

    private void OnDestroy()
    {
//        Services.Container.Resolve<OverkillService>().OnNodeDeath -= Track;
    }

    public void Track(IInteractableNode obj)
    {
        trackedOverkill = obj.NodeData.Uuid;
        var overKillRecord = Services.Container.Resolve<OverkillService>().GetOverkillRecord(trackedOverkill);
        if (overKillRecord == null || overKillRecord.overkillInfo == null || !overKillRecord.overkillInfo.IsOverkill)
        {
            return;
        }

        overkillValue.text = string.Format(overkillFormatted, SkillExperienceTable.Format(overKillRecord.overkillInfo.OverkillAmount));
        var drops = Mathf.RoundToInt((overKillRecord.overkillInfo.OverkillLogarithim - 1f) * 100);
        bonusDropsLabel.gameObject.SetActive(drops > 0);
        bonusDropsLabel.text = string.Format(BonusDropsText, drops);
        Show();
    }

    void Show()
    {
        if (!isShowing)
        {
            transform.SetAsFirstSibling();
        }
        isShowing = true;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        fadeAnim = cg.DOFade(1f, .1f);
    }

    void Hide()
    {
        isShowing = false;
        fadeAnim?.Kill();
        fadeAnim = cg.DOFade(0f, .1f);
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    private const string overkillFormatted = "<size=75%>x</size>{0}";
    private const string BonusDropsText = "{0}% bonus drops";
    private const string TimeLeftText = "{0}s";
    private void Update()
    {
        if (!isShowing)
        {
            return;
        }
        var overKillRecord = Services.Container.Resolve<OverkillService>().GetOverkillRecord(trackedOverkill);
        if (overKillRecord == null || overKillRecord.overkillInfo == null || !overKillRecord.overkillInfo.IsOverkill)
        {
            Hide();
            return;
        }

        var timeLeftSeconds = Mathf.CeilToInt(overKillRecord.expireTime-Time.time);
        timeLeftLabel.text = string.Format(TimeLeftText, timeLeftSeconds);
        timeLeftLabel.gameObject.SetActive(timeLeftSeconds < 4);
    }
}

