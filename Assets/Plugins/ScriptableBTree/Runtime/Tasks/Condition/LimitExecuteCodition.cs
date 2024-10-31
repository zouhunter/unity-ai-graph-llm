/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 次数限制执行条件
 *_*/

namespace MateAI.ScriptableBehaviourTree.Condition
{
    /// <summary>
    /// 限制次数执行条件
    /// </summary>
    [NodePath("次数限制执行条件")]
    public class LimitExecuteCodition : ConditionNode
    {
        public int limitCount = 1;

        [UnityEngine.SerializeField]
        private Ref<int> executeCount;

        protected override bool CheckCondition()
        {
            if (executeCount.Value++ < limitCount)
            {
                return true;
            }
            return false;
        }
    }
}

