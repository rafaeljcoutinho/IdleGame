using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestSideViewItemList : MonoBehaviour
{
    [SerializeField] private List<QuestObjectiveItemList> questObjectiveItemList;
    [SerializeField] private TextMeshProUGUI questTitle;
    [SerializeField] private CanvasGroup canvasGroupToFadeWhenDestroy;
    [SerializeField] private float fadeTime;
    public void SetQuestData(QuestViewData data)
    {
        ResetObjectives();
        questTitle.text = data.title;
        for (int i = 0; i < data.objectives.Count; i++)
        {
            questObjectiveItemList[i].SetData(data.objectives[i]);

            if (!questObjectiveItemList[i].gameObject.activeInHierarchy)
                questObjectiveItemList[i].gameObject.SetActive(true);
        }
        if (data.allObjectivesCompleted)
        {
            questObjectiveItemList[^1].SetData(new ObjectiveViewData{
            });
            questObjectiveItemList[^1].gameObject.SetActive(true);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetChild(3) as RectTransform);
    }

    private void ResetObjectives(){
        for(int i = 0; i < questObjectiveItemList.Count; i++)
        {
            questObjectiveItemList[i].gameObject.SetActive(false);
        }
    }

    public void CompleteQuest()
    {
        float delay = questObjectiveItemList[^1].CompleteObjetive();
        DOVirtual.DelayedCall(delay, () => {
            canvasGroupToFadeWhenDestroy.DOFade(0f, fadeTime)
            .OnComplete(() => Destroy(gameObject));
        });
    }
}

public class QuestViewData
{
    public string title;
    public List<ObjectiveViewData> objectives;
    public bool allObjectivesCompleted;
    public string completeObjectivesText;
}