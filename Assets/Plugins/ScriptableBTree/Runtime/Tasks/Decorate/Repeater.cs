using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Decorates
{
    [AddComponentMenu("BehaviourTree/Decorate/Repeater")]
    public class Repeater : DecorateNode
    {
        [SerializeField,Tooltip("loop max count, -1 means ignore!")]
        private int _loopCount = 1;

        [SerializeField, Tooltip("should end if child on Failure!")]
        private AbortType _abortType;

        public enum AbortType
        {
            None,
            Success,
            Failure,
        }

        protected override Status OnUpdate(TreeInfo info)
        {
            var status = base.ExecuteChild(info);
            if (status == Status.Failure && _abortType == AbortType.Failure)
            {
                return Status.Success;
            }
            else if (status == Status.Success && _abortType == AbortType.Success)
            {
                return Status.Success;
            }
            if (_loopCount > 0 && --_loopCount == 0)
            {
                return Status.Success;
            }
            return Status.Running;
        }
    }
}
