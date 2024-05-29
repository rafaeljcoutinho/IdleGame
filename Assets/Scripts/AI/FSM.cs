using System;
using System.Collections.Generic;

public class FSM<T> where T: IFSMState
{
    private Dictionary<Type, T> stateByType;
    private T currentState;
    public Type CurrentState => currentState.GetType();
    
    public FSM(List<T> states, Type initialState)
    {
        stateByType = new Dictionary<Type, T>();
        foreach (var state in states)
        {
            stateByType.Add(state.GetType(), state);
        }

        currentState = stateByType[initialState];
    }

    public void GoToState(Type t)
    {
        if (!Compare(stateByType[t], currentState))
        {
            currentState.OnForceChange();
        }
        currentState = stateByType[t];
    }
    
    public bool Compare(T x, T y)
    {
        return EqualityComparer<T>.Default.Equals(x, y);
    }

    public T GetState(Type t)
    {
        return stateByType[t];
    }
    
    public G GetState<G>() where G : class, IFSMState
    {
        return stateByType[typeof(G)] as G;
    }
    
    public void Update(float dt)
    {
        var newState = currentState.Update(dt);
        currentState = stateByType[newState];
    }
}

public interface IFSMState
{
    public Type Update(float dt);
    public void OnForceChange();
}

public abstract class FSMBaseState : IFSMState
{
    public FSM<FSMBaseState> FSM { get; set; }
    public abstract Type Update(float dt);
    public virtual void OnForceChange() {}
}
