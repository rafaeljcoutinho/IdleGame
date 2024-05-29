using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AfkProgressPopup : MonoBehaviour
{
    public enum Result
    {
        Claim,
        WatchRewardedVideo,
    }
    public struct  ViewData
    {
        public string title;
        public string subtitle;
        public bool showVideoButton;
        public Dictionary<Item, long> drops;
        public string imageLabel;
        public Sprite leftImage;
    }
    
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI subTitle;
    [SerializeField] private Blocker Blocker;
    [SerializeField] private GameObject watchVideoButton;
    [SerializeField] private ItemIconPrefab itemIconPrefab;
    [SerializeField] private Transform itemRewardsContainer;
    [SerializeField] private Image leftIcon;
    [SerializeField] private TextMeshProUGUI kcLabel;

    public ViewData _ViewData;
    private Action<Result> callback;

    public void Show(Action<Result> callback)
    {
        this.callback = callback;
        title.text = _ViewData.title;
        subTitle.text = _ViewData.subtitle;

        //todo: object pool
        for (var i = itemRewardsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemRewardsContainer.GetChild(i).gameObject);
        }

        foreach (var kvPair in _ViewData.drops)
        {
            var item = kvPair.Key;
            var quantity = kvPair.Value;
            var instance = Instantiate(itemIconPrefab, itemRewardsContainer);
            instance.SetData(new InventoryItemViewData
            {
                itemData = item,
                itemQuantity = quantity,
            });
            instance.SetButton(ItemClick);
        }
        
        gameObject.SetActive(true);
        watchVideoButton.SetActive(_ViewData.showVideoButton);
        leftIcon.gameObject.SetActive(_ViewData.leftImage != null);
        if (_ViewData.leftImage != null)
        {
            leftIcon.sprite = _ViewData.leftImage;
        }
        kcLabel.text = _ViewData.imageLabel;
        
        Blocker.Show(null);
    }

    private void ItemClick(InventoryItemViewData item, Vector3 pos)
    {
        OverlayCanvas.Instance.ToolTip.Show(item.itemData, pos, ConsumableItemResolver.Source.Inventory, true);
    }

    public void Close()
    {
        callback = null;
        gameObject.SetActive(false);
        Blocker.Hide();
    }
    
    public void OnClaimButton()
    {
        callback?.Invoke(Result.Claim);
        Close();
    }

    public void OnWatchVideoButton()
    {
        callback?.Invoke(Result.WatchRewardedVideo);
        Close();
    }
}
