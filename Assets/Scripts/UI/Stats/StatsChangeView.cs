using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StatsChangeView : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI name; 
    [SerializeField] private TextMeshProUGUI stat; 
    [SerializeField] private TextMeshProUGUI statChange;

    private float animationProgress = 0f;
    private bool isShowing = false;

    private float delayStat;
    private float delayStatChange;

    private const string Format = "<color=#70E053>+{0}</color>";
    public void Show(SkillData.PrettyModifierInfo viewData, float delayStat, float delayStatChange)
    {
        name.text = viewData.Name;
        if (viewData.Delta > 0)
            statChange.text = string.Format(Format, viewData.DeltaFormatted);
        this.delayStat = delayStat;
        this.delayStatChange = delayStatChange;
    }

    private void Start()
    {
        StartCoroutine(Animate(delayStat));
        StartCoroutine(AnimateStatChange(delayStatChange));
    }

    IEnumerator Animate(float delay)
    {
        var time = 0f;
        ChangeAlpha(name, 0);
        ChangeAlpha(stat, 0);
        ChangeAlpha(statChange, 0);
        var duration = .7f;
        yield return new WaitForSeconds(delay);
        while (time < duration)
        {
            var factor = time / duration;
            ChangeAlpha(name, Mathf.SmoothStep(0,1, factor));
            ChangeAlpha(stat, Mathf.SmoothStep(0,1, factor));
            time += Time.deltaTime;
            yield return null;
        }
        ChangeAlpha(name, 1);
        ChangeAlpha(stat, 1);
    }

    void ChangeAlpha(TextMeshProUGUI t, float val)
    {
        var color = t.color;
        color.a = val;
        t.color = color;
    }
    
    IEnumerator AnimateStatChange(float delay)
    {
        var time = 0f;
        ChangeAlpha(statChange, 0);
        var duration = .7f;
        yield return new WaitForSeconds(delay);
        while (time < duration)
        {
            var factor = time / duration;
            ChangeAlpha(statChange, Mathf.SmoothStep(0,1, factor));
            time += Time.deltaTime;
            yield return null;
        }
        ChangeAlpha(statChange, 1);
    }
}
