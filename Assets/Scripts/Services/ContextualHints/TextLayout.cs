using System;
using TMPro;
using UnityEngine;

public class TextLayout : FloatingTipLayout
{
    [SerializeField] private TextMeshProUGUI textComponent;
    private LayoutData data;
    public class LayoutData : IFloatingTipLayoutData
    {
        public string LocalizedText;
        public ContextualHintService.LayoutType Type => ContextualHintService.LayoutType.SimpleText;
    }

    public override void Setup(IFloatingTipLayoutData data)
    {
        this.data = data as LayoutData;
        textComponent.text =  this.data.LocalizedText;
    }

    private void LateUpdate()
    {
        if (data != null)
            textComponent.text =  this.data.LocalizedText;
    }
}