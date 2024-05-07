using UnityEngine;
using UFrame.NodeGraph;

namespace AIScripting
{
    [CustomNode("Debug",group:"AIScripting")]
    public class DebugNode : ScriptNodeBase
    {
        public Ref<string> info;
        public LogType logType;

        protected override int InCount => int.MaxValue;

        protected override void OnProcess()
        {
            Debug.unityLogger.Log(logType, "[DebugNode]:" + info.Value);
            DoFinish(true);
        }
    }
}