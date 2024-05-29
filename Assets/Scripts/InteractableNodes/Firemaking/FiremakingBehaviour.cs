using System;
using UnityEngine;

namespace Firemaking
{
    public class FiremakingBehaviour : Player.Behaviour
    {
        public override Type Update(float dt)
        {
            return GetType();
        }

        public override void SetInteractableNode(IInteractableNode node)
        {
           
        }

        public override SkillData.Type SkillUsed => SkillData.Type.Firemaking;
    }
}