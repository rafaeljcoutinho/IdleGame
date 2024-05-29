using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextWithImageLayout : FloatingTipLayout
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Image image;

    public class LayoutData : IFloatingTipLayoutData
    {
        public string LocalizedText;
        public Sprite sprite;
        public ContextualHintService.LayoutType Type => ContextualHintService.LayoutType.TextWithImage;
    }

    public override void Setup(IFloatingTipLayoutData data)
    {
        var layoutData = data as LayoutData;
        textComponent.text =  layoutData.LocalizedText;
        //image.sprite = layoutData.sprite;
    }
}