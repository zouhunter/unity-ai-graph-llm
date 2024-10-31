/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 
 *_*/

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Composite
{
    public class ParallelNode : CompositeNode
    {
        [SerializeField]
        private MatchType _abortType;
        public MatchType abortType => _abortType;

        protected override Status OnUpdate(TreeInfo info)
        {
            var childCount = GetChildCount(info);
            if (childCount == 0)
                return Status.Inactive;

            var resultStatus = Status.Failure;
            var successCount = 0;
            var failureCount = 0;
            for (int i = 0; i < childCount; i++)
            {
                var child = GetChild(info, i);
                var childStatus = child.node?.Execute(child);
                if (childStatus == Status.Inactive)
                    continue;

                switch (childStatus)
                {
                    case Status.Inactive:
                        continue;
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        if(abortType == MatchType.AnyFailure)
                            resultStatus = Status.Success;
                        else if(abortType == MatchType.AllSuccess)
                            resultStatus = Status.Failure;
                        failureCount++;
                        break;
                    case Status.Success:
                        if(abortType == MatchType.AnySuccess)
                            resultStatus = Status.Success;
                        else if(abortType == MatchType.AllFailure)
                            resultStatus = Status.Failure;
                        successCount++;
                        break;
                    default:
                        break;
                }
            }
            if (abortType == MatchType.AllSuccess && successCount == GetChildCount(info))
                resultStatus = Status.Success;
            else if(abortType == MatchType.AllFailure && failureCount == GetChildCount(info))
                resultStatus = Status.Success;
            return resultStatus;
        }
    }
}
