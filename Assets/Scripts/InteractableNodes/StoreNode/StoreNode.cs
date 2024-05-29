using System;
using UnityEngine;
using Woodcutting;

public class StoreNode : BaseInteractableNodeMonobehaviour
{

    [SerializeField] private Store storeData;
    [SerializeField] private GameplaySceneBootstrapper gameplaySceneBootstrapper;
    
    public override NodeData NodeData => null;
    
    public override Type PlayerBehaviour => typeof(CloseDistanceBehaviour);

    public override void Interact()
    {
        base.Interact();
        gameplaySceneBootstrapper.Player.playerBehaviourFSM.GetState<CloseDistanceBehaviour>().SetTarget(transform, .6f,
            typeof(DoNothingBehaviour),
            () =>
            {
                Debug.Log("open store view - " + storeData.id);
                Services.Container.Resolve<InventoryService>().PlayerProfile.storeManager.Update(storeData);
                var storeViewData = Services.Container.Resolve<InventoryService>().PlayerProfile.storeManager.GetStoreViewData(storeData);
                OverlayCanvas.Instance.StoreView.Show(storeViewData, storeData);
            });
    }
                
}
