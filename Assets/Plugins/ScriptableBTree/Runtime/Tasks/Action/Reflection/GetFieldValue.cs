/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-14
 * Version: 1.0.0
 * Description: 反射获取字段值
 *_*/

using System.Collections.Generic;
using System.Reflection;

namespace MateAI.ScriptableBehaviourTree.Actions
{
    [NodePath("获取字段值")]
    public class GetFieldValue : ActionNode
    {
        public Ref<object> target;
        public Ref<string> fieldName;
        public Ref<object> result;
        public BindingFlags bindingFlags;

        protected override IEnumerable<IRef> GetRefVars()
        {
            return new IRef[] { target, fieldName, result };
        }

        protected override Status OnUpdate()
        {
            if (target.Value == null || string.IsNullOrEmpty(fieldName))
            {
                return Status.Failure;
            }
            var field = target.Value.GetType().GetField(fieldName, bindingFlags);
            if (field != null)
            {
                var value = field.GetValue(target.Value);
                result.SetValue(value);
                return Status.Success;
            }
            return Status.Failure;
        }
    }
}
