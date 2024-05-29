using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class InteractableMapElementUIAdapter : MonoBehaviour
    {
        [SerializeField] private Transform nodePoolRoot;
        [SerializeField] private InteractableNodeView prefab;
        [SerializeField] private RectTransform nodeContainer;

        [SerializeField] private List<Transform> nodeRoots;

        private ObjectPool<InteractableNodeView> nodes;
        private Vector3[][] worldCornerBuffers = new Vector3[2][];
        
        private void Awake()
        {
            nodes = new ObjectPool<InteractableNodeView>(nodePoolRoot, prefab);
            for (var i = 0; i < worldCornerBuffers.Length; i++)
                worldCornerBuffers[i] = new Vector3[4];
        }

        private void Start()
        {
            foreach (var t in nodeRoots)
            {
                var n = nodes.Pop();
                n.Bind(new InteractableNodeView.ViewData{Name = t.name});
                
                (n.transform as RectTransform).GetWorldCorners(worldCornerBuffers[0]);
                nodeContainer.GetWorldCorners(worldCornerBuffers[1]);

                Contextual2DObject.Utility.StayWithinBounds(0, 0,
                    worldCornerBuffers[0][0], worldCornerBuffers[1][0],
                    worldCornerBuffers[0][1], worldCornerBuffers[1][1],
                    n.transform as RectTransform, nodeContainer
                );
            }
        }
    }
}