/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-14
 * Version: 1.0.0
 * Description: 反射调用方法
 *_*/

using System.Collections.Generic;
using System.Reflection;

namespace MateAI.ScriptableBehaviourTree.Actions
{
    [NodePath("反射方法调用")]
    public class InvokeMethod : ActionNode
    {
        public Ref<object> target;
        public Ref<string> methodName;
        public Ref<object[]> args;
        public Ref<object> result;
        public BindingFlags bindingFlags;

        protected override IEnumerable<IRef> GetRefVars()
        {
            return new IRef[] { target, methodName, args , result };
        }

        protected override Status OnUpdate()
        {
            if (target.Value == null || string.IsNullOrEmpty(methodName))
            {
                return Status.Failure;
            }
            result.Value = target.Value.GetType().InvokeMember(methodName, bindingFlags, null, target.Value, args.Value);
            return Status.Success;
        }
    }
}
