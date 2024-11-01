/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 直到子节点返回失败
 *_*/

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Decorates
{
    [NodePath("直到失败=>{returnStatus}")]
    public class UntilFailure : DecorateNode
    {
        [PrimaryArg(Status.Success,Status.Failure)]
        public Status returnStatus;
        protected override Status OnUpdate(TreeInfo info)
        {
            var childResult = base.ExecuteChild(info);
            if (childResult == Status.Failure)
            {
                return returnStatus;
            }
            return Status.Running;
        }
    }
}
