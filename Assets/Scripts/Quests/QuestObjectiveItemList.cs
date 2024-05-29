using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuestObjectiveItemList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private Image completeSlider;
    [SerializeField] private CanvasGroup objectsToFade;
    [SerializeField] private float timeToFade;
    [SerializeField] private float timeToCheck;

    private bool isComplete = false;
    public void SetData(ObjectiveViewData data)
    {
        objectiveText.text = data.objectiveText;
        
        if(data.isComplete)
        {
            CompleteObjetive();
        }
        else
        {
            NotCompleteObjective();
        }
        gameObject.SetActive(true);
    }

    public float CompleteObjetive()
    {
        if (isComplete) return timeToCheck;

        completeSlider.DOFillAmount(1, timeToCheck);

        isComplete = true;
        return timeToCheck;
    }

    public float FadeOut()
    {
        objectsToFade.DOFade(0, timeToFade).OnComplete( () => {completeSlider.fillAmount = 0; isComplete = false;} );
        return timeToFade;
    }

    public float FadeIn()
    {
        objectsToFade.DOFade(1, timeToFade);
        return timeToFade;
    }

    public void NotCompleteObjective() 
    {
        if (!isComplete) return;

        completeSlider.DOFillAmount(0, timeToCheck);
        isComplete = false;
    }

    public void Reset()
    {
        completeSlider.DOFillAmount(0, 0);
        isComplete = false;
        gameObject.SetActive(false);
    }
}

public class ObjectiveViewData
{
    public string objectiveText;
    public bool isComplete;
}