using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineDispatcher : MonoBehaviour
{
    private Dictionary<float, WaitForSeconds> instructionPool;

    public CoroutineDispatcher()
    {
        instructionPool = new Dictionary<float, WaitForSeconds>();
    }

    public Coroutine Run(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    public void Stop(Coroutine routine)
    {
        StopCoroutine(routine);
    }

    public Coroutine AtEndOfFrame(Action action)
    {
        return Run(WaitRoutine(new WaitForEndOfFrame(), action));
    }

    public Coroutine OnNextFrame(Action action)
    {
        return Run(WaitRoutine(null, action));
    }

    public Coroutine AfterDelay(float seconds, Action action)
    {
        return Run(WaitRoutine(RetrievePooledWaitForSeconds(seconds), action));
    }
        
    private IEnumerator WaitRoutine(YieldInstruction instruction, Action action)
    {
        yield return instruction;
        action.Invoke();
    }

    private WaitForSeconds RetrievePooledWaitForSeconds(float duration)
    {
        if (!instructionPool.TryGetValue(duration, out var instruction))
        {
            instruction = new WaitForSeconds(duration);
            instructionPool.Add(duration, instruction);
        }

        return instruction;
    }
}