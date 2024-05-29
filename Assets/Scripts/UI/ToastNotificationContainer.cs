using System;
using System.Collections.Generic;
using UnityEngine;

public class ToastNotificationContainer : MonoBehaviour
{
    [Serializable]
    public class Prefabs
    {
        public ContextualHintService.LayoutType layoutType;
        public ToastNotificationItemView layout;
    }

    [SerializeField] private List<Prefabs> prefabs;
    [SerializeField] private Transform toastContainer;
    [SerializeField] private float messageCooldown;
    
    [Header("Calibration")] 
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform target;
    
    private Dictionary<ContextualHintService.LayoutType, ObjectPool<ToastNotificationItemView>> viewPoolDictionary;
    private Queue<IFloatingTipLayoutData> toShowQueuedItems;
    private float nextMessageLock;

    private void Awake()
    {
        viewPoolDictionary = new Dictionary<ContextualHintService.LayoutType, ObjectPool<ToastNotificationItemView>>();
        toShowQueuedItems = new Queue<IFloatingTipLayoutData>();
        foreach (var prefab in prefabs)
        {
            var poolContainer = new GameObject().transform;
            poolContainer.parent = toastContainer;
            viewPoolDictionary.Add(prefab.layoutType,new ObjectPool<ToastNotificationItemView>(poolContainer, prefab.layout));
        }
    }

    private void Update()
    {
        if (Time.time < nextMessageLock)
            return;
        if (toShowQueuedItems.Count == 0)
            return;
        var itemToShow = toShowQueuedItems.Dequeue();
        DoShow(itemToShow);
        nextMessageLock = Time.time + messageCooldown;
    }

    void DoShow(IFloatingTipLayoutData layoutData)
    {
        var layoutPrefab = viewPoolDictionary[layoutData.Type].Pop();
        layoutPrefab.Reset();
        layoutPrefab.UpdateLayoutData(layoutData);
        layoutPrefab.Show(spawnPoint, target, () =>
        {
            layoutPrefab.gameObject.SetActive(false);
            viewPoolDictionary[layoutData.Type].Push(layoutPrefab);
        });
    }

    public void Show(IFloatingTipLayoutData layout)
    {
        if (toShowQueuedItems.Count < 2)
            toShowQueuedItems.Enqueue(layout);
    }
}
