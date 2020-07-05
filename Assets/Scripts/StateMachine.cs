using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class StateMachine
{
    private IState currentState;
    private IState previousState;

    public void ChangeState(IState newState)
    {
        if (this.currentState != null)
        {
            this.currentState.Exit();
        }
        this.previousState = this.currentState;
        this.currentState = newState;
        this.currentState.Enter();
    }

    public void ExecuteStateUpdate()
    {
        if (this.currentState != null)
        {
            this.currentState.Execute();
        }
    }

    public void SwitchToPreviousState()
    {
        this.currentState.Exit();
        this.currentState = this.previousState;
        this.currentState.Enter();
    }
}

