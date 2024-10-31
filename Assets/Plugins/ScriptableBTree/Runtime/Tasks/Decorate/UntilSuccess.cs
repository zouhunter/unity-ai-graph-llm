/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 直到子节点返回成功
 *_*/

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Decorates
{
    [AddComponentMenu("BehaviourTree/Decorate/UntilSuccess")]
    public class UntilSuccess : DecorateNode
    {
        protected override Status OnUpdate(TreeInfo info)
        {
            var childResult = base.ExecuteChild(info);
            if (childResult == Status.Success)
            {
                return Status.Success;
            }
            return Status.Running;
        }
    }
}
