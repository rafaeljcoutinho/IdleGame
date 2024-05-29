using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class ResetWallet : MonoBehaviour
{
    public void ResetW() {
        Services.Container.Resolve<InventoryService>().Reset();
    }
}
