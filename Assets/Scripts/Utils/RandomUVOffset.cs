using UnityEngine;
using Random = UnityEngine.Random;

public class RandomUVOffset : MonoBehaviour
{
    [SerializeField] private MeshRenderer msr;

    private void Start()
    {
        ChangeColors();
    }

    [ContextMenu("Change colors")]
    void ChangeColors()
    {
        if (msr == null) return;
        var materialMainTextureOffset = msr.material.mainTextureOffset;
        materialMainTextureOffset.x = Random.Range(0, msr.material.mainTextureScale.x * .5f);
        msr.material.mainTextureOffset = materialMainTextureOffset;
    }
}
