using System;
using UnityEngine;
using UnityEngine.UI;

public class FoodSelectionView : MonoBehaviour
{
    [SerializeField] private Transform container;

    private Action<EquipableFood> callback;

    public void Show(Action<EquipableFood> callback)
    {
        this.callback = callback;
        Initialize();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        callback = null;
        gameObject.SetActive(false);
    }

    private void Initialize()
    {
        for (var i = container.childCount-1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
        var itemDatabase = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase;
        foreach (var kvPair in Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet.Items)
        {
            var id = kvPair.Key;
            var item = itemDatabase.GetItem(id);
            if (kvPair.Value == 0)
                continue;
            if (item is not EquipableFood food)
            {
                continue;
            }

            var instance = new GameObject($"food_{food.name}");
            var rectT = instance.AddComponent<RectTransform>();
            var image = instance.AddComponent<Image>();
            var button = instance.AddComponent<Button>();
            rectT.sizeDelta = new Vector2(100, 100);
            instance.transform.SetParent(container);
            button.onClick.AddListener(() => { OnClick(food);});
            image.sprite = food.SmallThumbnail;
        }
    }

    private void OnClick(EquipableFood obj)
    {
        callback?.Invoke(obj);
        Close();
    }
}
