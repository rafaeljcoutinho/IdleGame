using System;
using InteractableNodes;
using UnityEngine;
using UnityEngine.AI;

namespace DefaultNamespace
{
    public class MapClickListener : MonoBehaviour
    {
        [SerializeField] private LayerMask InteractableLayer;
        [SerializeField] private ParticleSystem ps;

        private Camera cam;
        public BaseInteractableNodeMonobehaviour Node {  get; set; }
        public Vector3 Position {  get; private set; }
        public bool HasClick {  get; private set; }

        public void SetCamera(Camera cam)
        {
            this.cam = cam;
        }
        
        void Update()
        {
            Node = null;
            Position = Vector3.zero;
            HasClick = false;
            if (Input.GetMouseButtonDown(0))
            {
                HasClick = true;
                OnMouseClick();
            }
        }

        private void OnMouseClick()
        {
            if (cam == null)
            {
                return;
            }

            if (OverlayCanvas.Instance.EventSystem.currentSelectedGameObject != null)
            {
                return;
            }

            var mouseRay = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(mouseRay, out var hitInfo, int.MaxValue, InteractableLayer))
                return;
            Debug.Log(hitInfo.collider.name);

            var nodeClickListener = hitInfo.collider.GetComponent<InteractableNodeClickListener>();
            if (nodeClickListener != null)
            {
                Node = nodeClickListener.NodeBehavior;
            }
            if (NavMesh.SamplePosition(hitInfo.point, out var navHit, .2f, NavMesh.AllAreas))
            {
                Position = navHit.position;
                if (Node == null)
                {
                    ps.transform.position = Position;
                    ps.Stop();
                    ps.Play();
                }
            }
            else
            {
                HasClick = Node != null;
            }
        }
    }
}