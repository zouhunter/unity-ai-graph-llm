/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 输出指定变量到控制台
 *_*/
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Actions
{
    [NodePath("变量日志")]
    public class VariableDebugger : ActionNode
    {
        public LogType logType;
        public string variableName;
        public string formatText = "{0}";

        protected override Status OnUpdate()
        {
            var variable = Owner.GetVariable(variableName);
            if (variable == null)
            {
                return Status.Failure;
            }
            Debug.unityLogger.LogFormat(logType, formatText, variable.GetValue()?.ToString());
            return Status.Success;
        }
    }
}
