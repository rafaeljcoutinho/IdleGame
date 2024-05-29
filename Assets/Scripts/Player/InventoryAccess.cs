
using UnityEngine;

public class InventoryAccess : MonoBehaviour
{
    private void OnEnable() {
        OverlayCanvas.Instance.InventoryViewController.Init();
    }

    private void OnDisable() {
        OverlayCanvas.Instance.InventoryViewController.Finish();    
    }
}
