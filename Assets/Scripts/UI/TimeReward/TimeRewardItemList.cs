
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeRewardItemList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Image icon;
    [SerializeField] private Button iconClick;

    public void SetData(Sprite icon, Action info)
    {
        this.icon.sprite = icon;
        iconClick.onClick.RemoveAllListeners();
        iconClick.onClick.AddListener(()=> info());
    }

    public void SetTime(string timer)
    {
        this.timer.text = timer;
    }

    public void EnableReward(bool isEnable)
    {
        if (isEnable)
            timer.text = "CLAIM";
    }
}

