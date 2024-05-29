using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Firemaking
{
    public class FirePlace : BaseInteractableNodeMonobehaviour
    {
        [SerializeField] private BonfireNodeData bonfireNodeData;
        //[SerializeField] private TextMeshProUGUI upgradeCostUI;
        //[SerializeField] private TextMeshProUGUI activeBuffs;
        //[SerializeField] private TextMeshProUGUI currentLevel;

        Wallet Wallet => Services.Container.Resolve<InventoryService>().PlayerProfile.Wallet;

        public override NodeData NodeData => bonfireNodeData;
        public override Type PlayerBehaviour => typeof(FiremakingBehaviour);

        

        private void Start()
        {
            //upgradeCostUI.text = bonfireNodeData.cost[bonfireNodeData.currentLv].ToString();
            //currentLevel.text = bonfireNodeData.currentLv.ToString();
            //activeBuffs.text = bonfireNodeData.reward[bonfireNodeData.currentLv].ToString();
        }
        public void Upgrade()
        {

            var costs = new Dictionary<Guid, long>();
            for(int i = 0; i<bonfireNodeData.cost.Count; i++)
            {
                costs.Add(bonfireNodeData.cost[i].item.Uuid, bonfireNodeData.cost[i].quantity);
            }
            Wallet.SpendItems(costs);
            ////consumir item do player
            ////dar reward
            //bonfireNodeData.currentLv++;
            ////aumentar custo

        }
        public override void Highlight()
        {
        }

        public override void DeHighlight()
        {
        }
    }
}

