using System;

namespace Slayer
{
    public class SlayerBehaviour : Player.Behaviour
    {
        private Player player;
        public override bool AllowFSMReset => false;

        public SlayerBehaviour(Player player)
        {
            this.player = player;
        }
        
        public override Type Update(float dt)
        {
            return GetType();
        }

        public override void SetInteractableNode(IInteractableNode node)
        {
            OverlayCanvas.Instance.SlayersLodgeView.Setup(node as SlayersLodgeNode);
            OverlayCanvas.Instance.SlayersLodgeView.OnHide = () =>
            {
                player.ResetFSM();
            };
            OverlayCanvas.Instance.SlayersLodgeView.Show();
        }

        public override SkillData.Type SkillUsed => SkillData.Type.None;
    }
}