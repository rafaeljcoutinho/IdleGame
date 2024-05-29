using System;
using UnityEngine;


namespace Cooking
{
    public class CookingNode : BaseInteractableNodeMonobehaviour
    {
        public override NodeData NodeData { get; }
        public override Type PlayerBehaviour => typeof(CookingBehaviour);
    }
}