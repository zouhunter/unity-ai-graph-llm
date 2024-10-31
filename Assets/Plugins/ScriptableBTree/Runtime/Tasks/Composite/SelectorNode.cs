/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 循序执行节点
 *_*/

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Composite
{
    [Icon("d32f060aec2e5df4cb1a4af839e5a832")]
    public class SelectorNode : CompositeNode
    {
        [SerializeField]
        private MatchType _abortType;
        public MatchType abortType => _abortType;

        protected override Status OnUpdate(TreeInfo info)
        {
            if (GetChildCount(info) == 0)
                return Status.Success;

            switch (abortType)
            {
                case MatchType.AllSuccess:
                    return CheckAllSuccess(info);
                case MatchType.AllFailure:
                    return CheckAllFailure(info);
                case MatchType.AnySuccess:
                    return CheckAnySuccess(info);
                case MatchType.AnyFailure:
                    return CheckAnyFailure(info);
            }
            return Status.Failure;
        }

        /// <summary>
        /// 检查全部成功
        /// </summary>
        /// <returns></returns>
        private Status CheckAllSuccess(TreeInfo info)
        {
            for (int i = 0; i < GetChildCount(info); i++)
            {
                var child = GetChild(info, i);
                var childStatus = child.node?.Execute(child) ?? Status.Inactive;
                if (childStatus == Status.Running)
                    return Status.Running;
                if(childStatus == Status.Failure)
                    return Status.Failure;
            }
            return Status.Success;
        }
        /// <summary>
        /// 检查任意成功
        /// </summary>
        /// <returns></returns>
        private Status CheckAnySuccess(TreeInfo info)
        {
            for (int i = 0; i < GetChildCount(info); i++)
            {
                var child = GetChild(info, i);
                var childStatus = child.node?.Execute(child) ?? Status.Inactive;
                if (childStatus == Status.Running)
                    return Status.Running;
                if (childStatus == Status.Success)
                    return Status.Success;
            }
            return Status.Failure;
        }
        /// <summary>
        /// 检查全部失败
        /// </summary>
        /// <returns></returns>
        private Status CheckAllFailure(TreeInfo info)
        {
            for (int i = 0; i < GetChildCount(info); i++)
            {
                var child = GetChild(info, i);
                var childStatus = child.node?.Execute(child) ?? Status.Inactive;
                if (childStatus == Status.Running)
                    return Status.Running;
                if (childStatus == Status.Success)
                    return Status.Failure;
            }
            return Status.Success;
        }
        /// <summary>
        /// 检查任意失败
        /// </summary>
        /// <returns></returns>
        private Status CheckAnyFailure(TreeInfo info)
        {
            for (int i = 0; i < GetChildCount(info); i++)
            {
                var child = GetChild(info, i);
                var childStatus = child.node?.Execute(child) ?? Status.Inactive;
                if (childStatus == Status.Running)
                    return Status.Running;
                if (childStatus == Status.Failure)
                    return Status.Success;
            }
            return Status.Failure;
        }

    }
}
