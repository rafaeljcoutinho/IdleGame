using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.UI;

public class QuestCTAView : MonoBehaviour
{
    [SerializeField] private Image exclamationMark;
    [SerializeField] private Image questionMark;
    [SerializeField] private Image completeQuestionMark;
    [SerializeField] private DisposableQuestCTAView cTAView;
    [SerializeField] private Transform container;
    
    private ObjectPool<DisposableQuestCTAView> disposableViewPool;

    private Dictionary<string, DisposableQuestCTAView> npcIcon = new();

    public void ShowCTA(string npcNameKey, Transform anchor, QuestIconType type)
    {
        if (disposableViewPool == null)
        {
            disposableViewPool = new ObjectPool<DisposableQuestCTAView>(container, cTAView);
        }
        if (!npcIcon.ContainsKey(npcNameKey))
        {
            if (type == QuestIconType.None) return;
            
            var ctaView = disposableViewPool.Pop();
            npcIcon.Add(npcNameKey, ctaView);
        }
        if (type == QuestIconType.None)
        {
            npcIcon[npcNameKey].gameObject.SetActive(false);
            disposableViewPool.Push(npcIcon[npcNameKey]);
            npcIcon.Remove(npcNameKey);
            return;
        }

        if (type == QuestIconType.ExclamationMark)
        {
            npcIcon[npcNameKey].Show(anchor, exclamationMark);
            npcIcon[npcNameKey].gameObject.SetActive(true);
        }
        else if (type == QuestIconType.QuestionMark)
        {
            npcIcon[npcNameKey].Show(anchor, questionMark);
            npcIcon[npcNameKey].gameObject.SetActive(true);
        }
        else if (type == QuestIconType.CompletedQuestionMark)
        {
            npcIcon[npcNameKey].Show(anchor, completeQuestionMark);
            npcIcon[npcNameKey].gameObject.SetActive(true);
        }
    }

    public void ShowAnyCTA(Transform anchor, QuestIconType type)
    {
        
    }
}

public enum QuestIconType
{
    None,
    ExclamationMark,
    QuestionMark,
    CompletedQuestionMark,
    
}