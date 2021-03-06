﻿using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TaskCategory("Custom")]
public class TeamDisperseAction : Action
{
    public SharedTeamerList children;
    public SharedTeamState teamState;
    bool quenceComplete = false;

    public override void OnStart()
    {
        quenceComplete = false;

        DG.Tweening.Sequence quence = DOTween.Sequence();
        quence.AppendInterval(0.5f);
        //quence.AppendCallback(() => {
        //    for (int i = 0; i < children.Value.Count; i++)
        //    {
        //        Teamer child = children.Value[i];
        //        child.Move();
        //    }
        //});
        DG.Tweening.Sequence parallel = DOTween.Sequence();
        float radius = 5.0f;
        for (int i = 0; i < children.Value.Count; i++)
        {
            Teamer child = children.Value[i];
            float angle = Mathf.PI * 2 / children.Value.Count * i;
            parallel.Join(
                child.transform.DOLocalMove(new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius),
                0.2f));
        }
        quence.Append(parallel);

        //quence.AppendCallback(() => {
        //    for (int i = 0; i < children.Value.Count; i++)
        //    {
        //        Teamer child = children.Value[i];
        //        child.Idle();
        //    }
        //});

        quence.AppendInterval(0.2f).onComplete = () => {
            quenceComplete = true;
        };
        teamState.SetValue(TeamState.Idel);
    }

    public override TaskStatus OnUpdate()
    {
        if (quenceComplete)
        {
            teamState.SetValue(TeamState.Formation);
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Running;
        }
    }
}
