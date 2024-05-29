using System.Collections.Generic;
using UnityEngine;

public class InteractableNodeGroup : MonoBehaviour
{
    [SerializeField] private List<BaseInteractableNodeMonobehaviour> nodes;

    public List<BaseInteractableNodeMonobehaviour> Nodes
    {
        get => nodes;
        set => nodes = value;
    }
}