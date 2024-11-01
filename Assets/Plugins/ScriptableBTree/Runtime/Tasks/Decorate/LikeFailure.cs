/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 不论对错，都返回失败
 *_*/

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Decorates
{
    [NodePath("纠错")]
    public class LikeFailure : DecorateNode
    {
        protected override Status OnUpdate(TreeInfo info)
        {
            var status = base.ExecuteChild(info);
            if (status == Status.Success)
            {
                return Status.Failure;
            }
            return status;
        }
    }
}
