using System;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class InteractableNodeView : MonoBehaviour
    {
        [SerializeField] private Text nodeLabel;

        private ViewData cachedViewData;
        
        public class ViewData
        {
            public string Name;
            public Transform WorldObj;
        }

        public void Bind(ViewData viewData)
        {
            nodeLabel.text = viewData.Name;
            cachedViewData = viewData;
        }
        
        private void LateUpdate()
        {
            if (cachedViewData == null) return;
            //cachedViewData.WorldObj
        }
    }
}