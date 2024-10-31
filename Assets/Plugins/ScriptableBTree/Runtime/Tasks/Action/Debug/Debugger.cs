/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 输出指定文本到控制台
 *_*/

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Actions
{
    [NodePath("文本日志:{logType}")]
    public class Debugger : ActionNode
    {
        public LogType logType;
        public string text;

        protected override Status OnUpdate()
        {
            Debug.unityLogger.Log(logType, text);
            return Status.Success;
        }
    }
}
