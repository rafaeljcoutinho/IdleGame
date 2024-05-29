using System;
using UnityEngine;

public class DoNothingBehaviour : Player.Behaviour
{
    public override SkillData.Type SkillUsed => SkillData.Type.None;

    public override Type Update(float dt)
    {
        return typeof(DoNothingBehaviour);
    }

    public override void SetInteractableNode(IInteractableNode node)
    {
    }
}