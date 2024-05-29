using System;
using System.Collections;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDialogView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] private TextMeshProUGUI backText;
    [SerializeField] private Image speakerIcon;
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private Button turnObjectiveButton;
    [SerializeField] private Button turnObjectiveButtonHolder;
    [SerializeField] private Npc player;

    private bool isWritting = false;
    private string targetText;

    private Action<bool> OnNext;

    public void Close()
    {
        if (canvas.interactable)
        {
            canvas.DOFade(0, .1f);
            canvas.interactable = false;
            canvas.blocksRaycasts = false;   
        }
    }
    
    public void ShowObjective(QuestStep step, Action<bool> onNext)
    {
        var canTurnIn = Services.Container.Resolve<InventoryService>().PlayerProfile.QuestProgressContainer.CheckStepRequirements(step);
        turnObjectiveButton.gameObject.SetActive(true);
        turnObjectiveButtonHolder.gameObject.SetActive(true);
        turnObjectiveButton.interactable = canTurnIn;
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
        canvas.DOFade(1f, .1f);
        var sb = new StringBuilder();
        foreach (var objective in step.Objectives)
        {
            var objectiveViewData = QuestTrackerManager.GetObjectiveViewData(objective, ": ");

            sb.Append(objectiveViewData.objectiveText);
            sb.AppendLine();
        }

        targetText = sb.ToString();
        OnNext = onNext;
        speakerIcon.sprite = step.npc.sprite;
        backText.text = targetText;
        StartCoroutine(TextWriter(targetText));
    }

    public void ShowDialog(QuestDialog dialog, QuestStep questStep, Action<bool> OnNext)
    {
        turnObjectiveButton.gameObject.SetActive(false);
        turnObjectiveButtonHolder.gameObject.SetActive(false);
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
        canvas.DOFade(1f, .1f);
        var dialogContent = Services.Container.Resolve<LocalizationService>().LocalizeText(dialog.textKey);
        targetText = dialogContent;

        if (!dialog.IsPlayerSpeaking)
        {
            speakerIcon.sprite = questStep.npc.sprite;
        } 
        else
        {
            speakerIcon.sprite = player.sprite;
        }

        this.OnNext = OnNext;
        backText.text = targetText;
        StartCoroutine(TextWriter(dialogContent));
    }

    public void OnClick()
    {
        if (isWritting)
        {
            StopAllCoroutines();
            Text.text = targetText;
            isWritting = false;
        }
        else
        {
            OnNext?.Invoke(false);
        }
    }

    public void OnTurnInClick()
    {
        OnNext?.Invoke(true);
    }
    
    IEnumerator TextWriter(string text)
    {
        Text.text = "";
        isWritting = true;
        foreach (var word in text)
        {
            Text.text += word;
            yield return null;
        }

        isWritting = false;
    }
}
