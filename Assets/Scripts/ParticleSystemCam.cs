
using DG.Tweening;
using UnityEngine;

public class ParticleSystemCam : MonoBehaviour
{
    [SerializeField] private ParticleSystem particle;


    public void Show()
    {
        particle.gameObject.SetActive(true);
        particle.Play();
    }

    public void Hide()
    {
        particle.Stop();
        DOVirtual.DelayedCall(1.5f, ()=> particle.gameObject.SetActive(false));
    }
    
}
