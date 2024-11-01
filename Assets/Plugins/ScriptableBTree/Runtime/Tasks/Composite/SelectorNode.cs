/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 顺序执行，选中返回成功
 *_*/

using System.Collections.Generic;

namespace MateAI.ScriptableBehaviourTree.Composite
{
    public class SelectorNode : CompositeNode
    {
        protected override Status OnUpdate(TreeInfo info)
        {
            var subIndex = info.subIndex;
            var status = Selector(info.subTrees,ref subIndex);
            info.subIndex = subIndex;
            return status;
        }

        protected Status Selector(List<TreeInfo> subTrees,ref int subIndex)
        {
            if (subTrees == null || subTrees.Count == 0)
                return Status.Inactive;

            var status = Status.Inactive;
            for (; subIndex < subTrees.Count; subIndex++)
            {
                var child = subTrees[subIndex];
                if (!child.enable || child.node == null)
                    continue;

                var childStatus = child.node.Execute(child);
                switch (childStatus)
                {
                    case Status.Inactive:
                        break;
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        if (matchStatus == Status.Failure)
                            return Status.Success;
                        status = Status.Failure;
                        break;
                    case Status.Success:
                        if (matchStatus == Status.Success)
                            return Status.Success;
                        status = Status.Failure;
                        break;
                    case Status.Interrupt:
                        return Status.Interrupt;
                    default:
                        break;
                }
            }
            return status;
        }
    }
}
