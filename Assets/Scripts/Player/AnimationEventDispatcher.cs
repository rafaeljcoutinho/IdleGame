using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventDispatcher : MonoBehaviour
{
    public event Action OnAttackAnimationEvent;
    public event Action OnChopAnimationEvent;
    public event Action OnFishingAnimationEvent;

    private List<Action> eventsToTrigger = new ();
    
    public void TriggerEvents()
    {
        foreach (var ev in eventsToTrigger)
        {
            ev?.Invoke();
        }
        eventsToTrigger.Clear();
    }
    
    public void AttackAnimationTrigger()
    {
        eventsToTrigger.Add(OnAttackAnimationEvent);
    }

    public void ChopTreeAnimationTrigger()
    {
        eventsToTrigger.Add(OnChopAnimationEvent);
    }

    public void FishingAnimationTrigger()
    {
        eventsToTrigger.Add(OnFishingAnimationEvent);
    }
}
