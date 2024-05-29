using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextualHintService
{
    [Serializable]
    public class HintDisplayOptions
    {
        public float offset = 0f;
        public Contextual2DObject.AnchorPosition anchorPosition = Contextual2DObject.AnchorPosition.Auto;
        public bool fadeBackground;
        public bool dismissOnScreenInteraction;
        public bool isDarkMode;
        public float autoHideSeconds;
    }
    
    public enum LayoutType
    {
        SimpleText,
        TextWithImage,
        SeasonPassReminder
    }

    private Contextual2DObject overlay;
    private CoroutineDispatcher CoroutineRunner => Services.Container.Resolve<CoroutineDispatcher>();
    private Coroutine displayCoroutine;

    private HintDatabase hintDatabase;
    private Dictionary<LayoutType, FloatingTipLayout> layoutPrefabs => hintDatabase.Layouts;
    private Dictionary<LayoutType, FloatingTipLayout> layoutInstances = new ();

    public Contextual2DObject Overlay => overlay;
    
    public void Load(Action<bool> callback)
    {
        var loadOp = Resources.LoadAsync<HintDatabase>("HintDatabase");
        loadOp.completed += operation =>
        {
            if (operation.isDone)
            {
                hintDatabase = loadOp.asset as HintDatabase;
            }
            hintDatabase.Start();
            callback?.Invoke(true);
        };
    }

    public void RegisterOverlay(Contextual2DObject overlay)
    {
        this.overlay = overlay;
    }
    
    private FloatingTipLayout GetOrCreatePrefab(LayoutType type)
    {
        if (layoutInstances.ContainsKey(type)) return layoutInstances[type];

        var instance = GameObject.Instantiate(layoutPrefabs[type], overlay.BubbleRect).GetComponent<FloatingTipLayout>();
        instance.transform.SetAsLastSibling();
        layoutInstances.Add(type, instance);

        var constraintLayout = instance.GetComponent<ConstrainedSizeFitter>(); 
        if (constraintLayout != null)
        {
            constraintLayout.SetConstrainedWithin(overlay.Root);
        }
        return layoutInstances[type];
    }

        public void Show(IFloatingTipLayoutData layoutData)
        {
            CleanupDisplayRoutine();
            SetupLayoutObject(layoutData);
            overlay.Show();
        }

        public void Show(IFloatingTipLayoutData layoutData, float delay, Action onShow)
        {
            CleanupDisplayRoutine();
            if (overlay.IsShowing) overlay.Hide();
            SetupLayoutObject(layoutData);
            displayCoroutine = CoroutineRunner.StartCoroutine(overlay.ShowDelayed(delay, onShow));
        }
        
        public void ShowWhenVisible(IFloatingTipLayoutData layoutData, float delay, Action onShow)
        {
            CleanupDisplayRoutine();
            if (overlay.IsShowing) overlay.Hide();
            SetupLayoutObject(layoutData);
            displayCoroutine = CoroutineRunner.StartCoroutine(overlay.ShowWhenVisible(delay, onShow));
        }

        public ContextualHintService SetTargetRectTransform(RectTransform target)
        {
            overlay.TargetRectTransform = target;
            overlay.TargetTransform = null;
            return this;
        }

        public ContextualHintService SetDisplayOptions(HintDisplayOptions displayOptions)
        {
            overlay.SetDisplayOptions(displayOptions);
            return this;
        }
        
        public ContextualHintService SetTargetTransform(Transform target)
        {
            overlay.TargetTransform = target;
            return this;
        }

        public void Hide(bool instant)
        {
            CleanupDisplayRoutine();
            overlay.Hide(instant);
        }

        private void CleanupDisplayRoutine ()
        {
            if (displayCoroutine != null)
            {
                CoroutineRunner.StopCoroutine(displayCoroutine);
                displayCoroutine = null;
            }
        }

        private void SetupLayoutObject(IFloatingTipLayoutData layoutData)
        {
            var prefab = GetOrCreatePrefab(layoutData.Type);
            DisableLayoutInstances();
            prefab.gameObject.SetActive(true);
            prefab.Setup(layoutData);
            LayoutRebuilder.ForceRebuildLayoutImmediate(overlay.BubbleRect);
        }

        void DisableLayoutInstances()
        {
            foreach (var kvPair in layoutInstances)
            {
                kvPair.Value.gameObject.SetActive(false);
            }
        }

        public bool IsShowing()
        {
            return overlay.IsShowing;
        }
    
}