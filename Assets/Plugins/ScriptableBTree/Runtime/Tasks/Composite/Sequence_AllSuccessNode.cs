/*-*-* Copyright (c) mateai@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 
 *_*/
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Composite
{
    [NodePath("顺序全部通过")]
    public class Sequence_AllSuccessNode : CompositeNode
    {
        protected override Status OnUpdate(TreeInfo info)
        {
            if (GetChildCount(info) == 0)
                return Status.Success;

            for (int i = 0; i < GetChildCount(info); i++)
            {
                var child = GetChild(info, i);
                var childStatus = child.node?.Execute(child) ?? Status.Inactive;
                if(childStatus == Status.Running)
                    return Status.Running;
                else if(childStatus == Status.Failure)
                    return Status.Failure;
            }
            return Status.Success;
        }
    }
}
