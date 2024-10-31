/*-*-* Copyright (c) mateai@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 
 *_*/
using System;
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Composite
{
    [NodePath("顺序任选通过")]
    public class Sequence_AnySuccessNode : CompositeNode
    {
        protected int runningIndex;
        protected int childCount;
        protected override void OnReset()
        {
            base.OnReset();
            runningIndex = 0;
        }

        protected override void OnStart(TreeInfo info)
        {
            base.OnStart();
            childCount = GetChildCount(info);
        }

        protected override Status OnUpdate(TreeInfo info)
        {
            if (childCount == 0)
                return Status.Inactive;

            for (int i = 0; i < GetChildCount(info); i++)
            {
                var child = GetChild(info,i);
                var childStatus = child.node?.Execute(child) ?? Status.Inactive;
                if (childStatus == Status.Running)
                    return Status.Running;
                if(childStatus == Status.Success)
                    return Status.Success;
            }
            return Status.Failure;
        }
    }
}
