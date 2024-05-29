using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextItemList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;
    [SerializeField] private Image circle;

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void SetTextColor(Color color)
    {
        text.color = color;
    }

    public void DisableCircle()
    {
        circle.gameObject.SetActive(false);
    }

    public void SetButtonAction(Action action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener( ()=> action() );
    }
}
