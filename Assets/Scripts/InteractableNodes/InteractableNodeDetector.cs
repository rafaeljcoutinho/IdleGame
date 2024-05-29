using System.Collections.Generic;
using UnityEngine;

public interface IInteractableNode
{
    public Transform transform { get; }
    public void Highlight();
    public void DeHighlight();
    public System.Type PlayerBehaviour { get; }
    public NodeData NodeData { get; }
    public void Interact();
}

public class InteractableNodeDetector : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private CapsuleCollider capsuleCollider;
    private Collider[] colliders = new Collider[16];

    private List<IInteractableNode> nearbyNodes;

    void Start()
    {
        nearbyNodes = new List<IInteractableNode>(16);
    }
    
    public void DetectSurroundings()
    {
        var count = capsuleCollider.OverlapCapsuleNonAlloc(transform, layerMask, colliders);
        nearbyNodes.Clear();
        for (var i = 0; i < count; i++)
        {
            if (colliders[i].transform == transform) // skip self
                continue;
            var yDistance = colliders[i].transform.position.y - transform.position.y;

            // node is on another height level, dont consider it
            if (Mathf.Abs(yDistance) > 0.25f)
            {
                continue;
            }

            var interactableNode = colliders[i].GetComponent<IInteractableNode>();
            if (interactableNode != null)
            {
                nearbyNodes.Add(interactableNode);
            }
        }
    }

    public IInteractableNode ClosestNode()
    {
        if (nearbyNodes.Count == 0) return null;

        var closest = nearbyNodes[0];
        var dist = Vector3.Distance(nearbyNodes[0].transform.position.HorizontalPlane(),
            transform.position.HorizontalPlane());

        foreach (var node in nearbyNodes)
        {
            var newDist = Vector3.Distance(node.transform.position.HorizontalPlane(),
                transform.position.HorizontalPlane());
            if (newDist < dist)
            {
                dist = newDist;
                closest = node;
            }
        }

        return closest;
    }
    
}
