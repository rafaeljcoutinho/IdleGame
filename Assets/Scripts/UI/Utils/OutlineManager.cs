using UnityEngine;

public class OutlineManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem outline;

    private Transform target;
    
    private void LateUpdate()
    {
        if (target != null)
        {
            outline.transform.position = target.position;
        }
    }

    public void Show(Transform parent, Color color)
    {
        target = parent;
        outline.transform.position = parent.position;
        outline.transform.localScale = parent.lossyScale;
        var main = outline.main;
        main.startColor = color;
        outline.gameObject.SetActive(true);
    }

    public void Hide()
    {
        target = null;
        outline.gameObject.SetActive(false);
    }
}
