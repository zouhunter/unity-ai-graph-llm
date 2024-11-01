/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-14
 * Version: 1.0.0
 * Description: 反射设置属性值
 *_*/

using System.Collections.Generic;
using System.Reflection;

namespace MateAI.ScriptableBehaviourTree.Actions
{
    [NodePath("反射/设置属性值")]
    public class SetPropertyValue : ActionNode
    {
        public Ref<object> target;
        public Ref<string> propName;
        public Ref<object> result;
        public BindingFlags bindingFlags;

        protected override IEnumerable<IRef> GetRefVars()
        {
            return new IRef[] { target, propName, result };
        }

        protected override Status OnUpdate()
        {
            if (target.Value == null || string.IsNullOrEmpty(propName))
            {
                return Status.Failure;
            }
            var prop = target.Value.GetType().GetProperty(propName, bindingFlags);
            if (prop != null)
            {
                prop.SetValue(target.Value, result.Value);
                return Status.Success;
            }
            return Status.Failure;
        }
    }
}
