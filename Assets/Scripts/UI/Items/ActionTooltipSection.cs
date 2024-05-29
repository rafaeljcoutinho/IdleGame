using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionTooltipSection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI quantity;
    [SerializeField] private TextMeshProUGUI income;
    [SerializeField] private Image incomeIcon;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private Button actionButton;
    [SerializeField] private Slider slider;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject backButtonContainer;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button minusButtonGfx;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button plusButtonGfx;
    [SerializeField] private Color useColor;
    [SerializeField] private Color sellColor;
    [SerializeField] private Color buyColor;
    [SerializeField] private Color actionButtonLabelEnabled;
    [SerializeField] private Color actionButtonLabelDisabled;

    private long currentItemPrice;
    private long quantityEnabled;

    private const string SELL_TEXT = "SELL";
    private const string BUY_TEXT = "BUY";
    private const string USE_TEXT = "USE";


    public void Show(ItemWithQuantity priceItem, long totalQuantity, long realQuantity, Action<ItemWithQuantity, long> action, Action<bool> backAction, bool backButtonEnable = true)
    {
        Reset();
        currentItemPrice = priceItem.quantity;
        incomeIcon.sprite = priceItem.item.SmallThumbnail;
        slider.maxValue = totalQuantity > realQuantity? realQuantity : totalQuantity;
        slider.maxValue = slider.maxValue < 1 ? 1 : slider.maxValue;
        quantityEnabled = realQuantity;
        backAction?.Invoke(false);

        slider.minValue = slider.maxValue > 1 ?  1 :  0;

        backButtonContainer.SetActive(backButtonEnable);

        actionButton.onClick.AddListener( ()=> { action( priceItem, (long)slider.value); Hide(); });
        backButton.onClick.AddListener( ()=> {Hide(); backAction?.Invoke(true);} );
        minusButton.onClick.AddListener( ()=> { slider.value--; });
        plusButton.onClick.AddListener( ()=> { slider.value++; });       

        minusButton.interactable = slider.value > slider.minValue;
        plusButton.interactable = slider.value < slider.maxValue;
        Update();

        gameObject.SetActive(true);
    }

    private void Update() {
        slider.value = slider.value < 1? 1 : slider.value;

        slider.onValueChanged?.Invoke(Refresh(slider.value));
    }

    public void SetActionType(ActionType ActionType)
    {
        if (ActionType == ActionType.Sell)
        {
            actionText.text = SELL_TEXT;
            actionButton.image.color = sellColor;
        }
        if (ActionType == ActionType.Use)
        {
            actionText.text = USE_TEXT;
            actionButton.image.color = useColor;
        }
        if (ActionType == ActionType.Buy)
        {
            actionText.text = BUY_TEXT;
            actionButton.image.color = buyColor;
        }
    }
    

    private float Refresh(float value)
    {

        minusButton.interactable = slider.value > slider.minValue;
        plusButton.interactable = slider.value < slider.maxValue;


        if(slider.maxValue == 1)
        {
            minusButton.interactable = false;
            plusButton.interactable = false;
        }

        minusButtonGfx.interactable = minusButton.interactable;
        plusButtonGfx.interactable = plusButton.interactable;

        actionButton.interactable = slider.value <= quantityEnabled;

        if (actionButton.interactable)
        {
            income.color = actionButtonLabelEnabled;
        }
        else
        {
            income.color = actionButtonLabelDisabled;
        }
        var formatedValue = SkillExperienceTable.Format(slider.value * currentItemPrice);
        income.text = formatedValue;
        quantity.text = SkillExperienceTable.Format(value);
        return value;
    }

    private void Reset()
    {
        slider.value = 0;
        currentItemPrice = 0;
        actionButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
        minusButton.onClick.RemoveAllListeners();
        plusButton.onClick.RemoveAllListeners();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Reset();
    }


}

public enum ActionType{
    Sell,
    Buy,
    Use,
}