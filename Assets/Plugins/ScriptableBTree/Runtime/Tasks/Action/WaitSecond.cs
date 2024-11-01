/*-*-*-*
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Description: wait for seconds
 *-*-*/
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Actions
{
    [NodePath("�ȴ���ʱ(��)")]
    public class WaitSecond : ActionNode
    {
        public Ref<float> waitSecond = new Ref<float> { Value = 1f };
        private float _triggerTime;

        protected void ResetTime()
        {
            _triggerTime = Time.time + waitSecond.Value;
        }

        protected override void OnStart()
        {
            ResetTime();
        }

        protected override Status OnUpdate()
        {
            if (_triggerTime < Time.time)
            {
                ResetTime();
                return Status.Success;
            }
            return Status.Running;
        }
    }
}
