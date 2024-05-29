using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HintDatabase", menuName = "ContextualHint/Database")]
public class HintDatabase : ScriptableObject
{
    [SerializeField] private List<Pair> layouts;
    public Dictionary<ContextualHintService.LayoutType, FloatingTipLayout> Layouts;
    
    [Serializable]
    public class Pair
    {
        public ContextualHintService.LayoutType Type;
        public FloatingTipLayout Layout;
    }
    
    public void Start()
    {
        Layouts = new ();
        foreach (var item in layouts)
        {
            Layouts.Add(item.Type, item.Layout);
        }
    }
}