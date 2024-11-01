/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 直到子节点返回成功
 *_*/

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Decorates
{
    [NodePath("直到成功=>{returnStatus}")]
    public class UntilSuccess : DecorateNode
    {
        [PrimaryArg(Status.Success, Status.Failure)]
        public Status returnStatus;

        protected override Status OnUpdate(TreeInfo info)
        {
            var childResult = base.ExecuteChild(info);
            if (childResult == Status.Success)
            {
                return returnStatus;
            }
            return Status.Running;
        }
    }
}
