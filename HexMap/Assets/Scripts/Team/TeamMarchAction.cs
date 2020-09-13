using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TaskCategory("Custom")]
public class TeamMarchAction : Action
{
    public SharedTeamerList children;
    public SharedTeamState teamState;
    bool quenceComplete = false;

    public override void OnStart()
    {
        quenceComplete = false;
        DG.Tweening.Sequence quence = DOTween.Sequence();
        float radius = 3.0f;
        for (int i = 0; i < children.Value.Count; i++)
        {
            Teamer child = children.Value[i];
            float angle = Mathf.PI * 2 / children.Value.Count * i;
            quence.Join(
                child.transform.DOLocalMove(new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius),
                0.2f)).onComplete = () => {
                    quenceComplete = true;
                };
        }
        teamState.SetValue(TeamState.Idel);
    }

    public override TaskStatus OnUpdate()
    {
        if (quenceComplete)
        {
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Running;
        }
    }
}

