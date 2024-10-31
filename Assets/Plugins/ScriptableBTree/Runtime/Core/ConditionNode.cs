using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    /// <summary>
    /// 条件节点 (只返回成功或失败)
    /// </summary>
    public abstract class ConditionNode : BaseNode
    {
        protected abstract bool CheckCondition();

        protected override Status OnUpdate()
        {
            if (CheckCondition())
            {
                return Status.Success;
            }
            else
            {
                return Status.Failure;
            }
        }
    }
}
