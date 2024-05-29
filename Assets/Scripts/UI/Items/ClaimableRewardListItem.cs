using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClaimableRewardListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Button claimButton;
    [SerializeField] private TextMeshProUGUI claimButtonLabel;
    [SerializeField] private ItemIconPrefab itemIconPrefab;
    [SerializeField] private GameObject blockFilterToViewPrefab;
    [SerializeField] private TextMeshProUGUI descriptionWithImage;
    [SerializeField] private GameObject glow;


    public GameObject ItemIconPrefab => itemIconPrefab.gameObject;

    public void SetupText(string title, string description)
    {
        this.title.text = title;
        this.description.text = description;
    }


    public void SetupButton(Action action)
    {
        claimButton.onClick.AddListener(() => {action();});
    }

    public void SetClaimButtonText(bool isClaimed, bool isLocked)
    {
        claimButton.GetComponent<Image>().enabled = !isClaimed && !isLocked;
        claimButtonLabel.text = isClaimed ? "CLAIMED" : isLocked ? "LOCKED" : "CLAIM";
        claimButton.interactable = !isClaimed && !isLocked;
        glow.SetActive(!isClaimed && !isLocked);
    }

    public void EnableToClaim()
    {
        blockFilterToViewPrefab.SetActive(false);
    }

    private const string ITEM_WITH_QUANTITY = "{0} x{1}";

    public void SetItemIconPrefab(InventoryItemViewData inventoryItemViewData)
    {
        descriptionWithImage.text = String.Format(ITEM_WITH_QUANTITY,inventoryItemViewData.itemData.LocalizationKey, SkillExperienceTable.Format(inventoryItemViewData.itemQuantity));
        descriptionWithImage.gameObject.SetActive(true);
        description.gameObject.SetActive(false);
        itemIconPrefab.SetData(inventoryItemViewData);
        itemIconPrefab.SetQuantity("");
        itemIconPrefab.gameObject.SetActive(true);
    }
}