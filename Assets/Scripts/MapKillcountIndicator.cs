using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapKillcountIndicator : DynamicUIIndicator
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI kcLabel;

    public void SetData(Sprite icon, string label)
    {
        this.icon.sprite = icon;
        kcLabel.text = label;
    }
}