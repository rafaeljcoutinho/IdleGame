using UnityEngine;

namespace InteractableNodes
{
    public class InteractableNodeClickListener : MonoBehaviour
    {
        [SerializeField] private BaseInteractableNodeMonobehaviour nodeBehavior;
        public BaseInteractableNodeMonobehaviour NodeBehavior => nodeBehavior;
    }
}