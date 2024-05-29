using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestItemList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private Button button;

    public void SetQuestName(string questName)
    {
        this.questName.text = questName;
    }

    public void SetButton(Action action)
    {
        button.onClick.AddListener(()=> action());
    }
}
