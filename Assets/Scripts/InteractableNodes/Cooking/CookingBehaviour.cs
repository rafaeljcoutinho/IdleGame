using System;

namespace Cooking
{
    public class CookingBehaviour : Player.Behaviour
    {
        private Player player;
        public CookingBehaviour(Player player)
        {
            this.player = player;
        }
        
        public override Type Update(float dt)
        {
            return typeof(DoNothingBehaviour);
        }

        public override void SetInteractableNode(IInteractableNode node)
        {
            OverlayCanvas.Instance.CookingPopupView.Setup();
            OverlayCanvas.Instance.CookingPopupView.Show();
        }

        public override SkillData.Type SkillUsed => SkillData.Type.Cooking;
    }
}