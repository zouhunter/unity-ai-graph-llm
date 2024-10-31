/*-*-*-*
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Description: wait for seconds
 *-*-*/

using System.Collections.Generic;
using MateAI.ScriptableBehaviourTree;
using MateAI;
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Actions
{
    [NodePath("WaitSecond")]
    public class WaitSecond : ActionNode
    {
        public Ref<float> waitTime = new Ref<float> { Value = 1f };
        public Ref<bool> randomWait = new Ref<bool> { Value = false };
        public Ref<float> randomWaitMin = new Ref<float> { Value = 1f };
        public Ref<float> randomWaitMax = new Ref<float> { Value = 3f };

        private float nextTime;

        protected override IEnumerable<IRef> GetRefVars()
        {
            return new IRef[] { waitTime, randomWait, randomWaitMin, randomWaitMax };
        }

        protected void ResetTime()
        {
            nextTime = Time.time + (randomWait.Value ? Random.Range(randomWaitMin.Value, randomWaitMax.Value) : waitTime.Value);
        }

        protected override void OnStart()
        {
            ResetTime();
        }

        protected override Status OnUpdate()
        {
            if (nextTime <= Time.time)
            {
                ResetTime();
                return Status.Success;
            }
            return Status.Running;
        }
    }
}
