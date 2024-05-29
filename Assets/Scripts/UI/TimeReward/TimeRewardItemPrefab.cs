using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeRewardItemPrefab : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemQuantityText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Button button;
    [SerializeField] private Image rarity;

    private ItemWithQuantity itemWithQuantity;

    public void SetData(ItemWithQuantity itemWithQuantity){
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = itemWithQuantity.item.SmallThumbnail;
        itemQuantityText.text = "x" + SkillExperienceTable.Format(itemWithQuantity.quantity);
        var itemRarity = itemWithQuantity.item.rarity;
        itemNameText.text = itemWithQuantity.item.LocalizationKey;
        rarity.color = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase.RarityColorPairs[(int)itemRarity].color;
        this.itemWithQuantity = itemWithQuantity;
    }
    public void SetButton(Action<ItemWithQuantity, Vector3> action){
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(()=> action(itemWithQuantity, transform.position));
    }

}
