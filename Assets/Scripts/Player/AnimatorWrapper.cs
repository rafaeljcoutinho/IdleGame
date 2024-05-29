using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimatorWrapper
{
    public Animator animator;
    private Dictionary<string, float> durations;

    public float GetAnimationDuration(string animationClipName)
    {
        if (durations == null)
        {
            return 0;
        }
        if(!durations.ContainsKey(animationClipName)){
            return 0;
        }
        return durations[animationClipName];
    }
    
    public void Init()
    {
        durations = new Dictionary<string, float>();
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            durations.Add(clip.name, clip.length);
        }
    }
}