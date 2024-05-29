using System;
using UnityEngine;

namespace Slayer
{
    public class SlayersLodgeNode : BaseInteractableNodeMonobehaviour
    {
        [SerializeField] public SlayersLodgeConfig slayersLodgeData;
        public override NodeData NodeData => null;
        public override Type PlayerBehaviour => typeof(SlayerBehaviour);
    }
}