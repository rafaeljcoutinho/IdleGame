using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDropFeedView : MonoBehaviour
{
    [SerializeField] private ItemDropView itemDropViewPrefab;
    [SerializeField] private Transform itemViewContainer;

    [Header("Calibration")]
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private float duration = 5f;

    private ObjectPool<ItemDropView> itemViewPool;

    private void Start()
    {
        itemViewPool = new ObjectPool<ItemDropView>(itemViewContainer, itemDropViewPrefab);
        itemViewPool.Prewarm(10);
    }

    [SerializeField] private float speed;
    private void Update()
    {
        foreach (var view in itemViewPool.UsedObjects)
        {
            var targetPos = spawnPoint.anchoredPosition3D + Vector3.up * view.viewData.index * 1 * (view.transform as RectTransform).rect.height;
            (view.transform as RectTransform).anchoredPosition3D = Vector3.Lerp((view.transform as RectTransform).anchoredPosition3D, targetPos, speed * Time.deltaTime); 
            if (Time.time > view.viewData.expiresAt && view.IsShowing)
            {
                view.Hide(() =>
                {
                    view.gameObject.SetActive(false);
                    itemViewPool.Push(view);
                });
            }
        }
    }

    private void CollapseIndexes()
    {
        foreach (var view in itemViewPool.UsedObjects)
        {
            view.viewData.index += 1;
            if (view.viewData.index >= 10 && view.IsShowing)
            {
                view.Hide(() =>
                {
                    view.gameObject.SetActive(false);
                    itemViewPool.Push(view);
                });
            }
        }
    }

    public void ShowItems(Dictionary<Guid, long> itemsDropped)
    {
        foreach (var itemDropped in itemsDropped)
        {
            CollapseIndexes();
            var item = Services.Container.Resolve<ItemDatabaseService>().ItemDatabase
                .GetItem(itemDropped.Key);
            var view = itemViewPool.Pop();
            view.viewData.amount = itemDropped.Value;
            view.viewData.text = item.LocalizationKey;
            view.viewData.expiresAt = Time.time + duration;
            view.viewData.item = item;
            view.viewData.index = 0;
            view.ApplyViewData();
            var pos = spawnPoint.transform.position;
            view.Show(pos, pos, null, false);
            (view.transform as RectTransform).anchoredPosition3D = spawnPoint.anchoredPosition3D;
        }
    }
}