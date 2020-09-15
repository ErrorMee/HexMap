using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamState 
{
    Idel,
    Formation,
    March,
    Disperse
}

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedTeamState : SharedVariable<TeamState>
    {
        public static implicit operator SharedTeamState(TeamState value) { return new SharedTeamState { mValue = value }; }
    }
}

[TaskCategory("Custom")]
public class CompareSharedTeamState : Conditional
{
    public SharedTeamState state;
    public SharedTeamState compareTo;

    public override TaskStatus OnUpdate()
    {
        if (state.Value == compareTo.Value)
        {
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        }
    }
}

