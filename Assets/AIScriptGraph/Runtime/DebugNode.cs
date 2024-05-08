using UnityEngine;
using UFrame.NodeGraph;

namespace AIScripting
{
    [CustomNode("Debug",group:"AIScripting")]
    public class DebugNode : ScriptNodeBase
    {
        public Ref<string> info;
        public string format;
        public LogType logType;

        protected override int InCount => int.MaxValue;

        protected override void OnProcess()
        {
            if(string.IsNullOrEmpty(format) || !format.Contains("{0}"))
            {
                Debug.unityLogger.Log(logType, "[DebugNode]:" + info.Value);
            }
            else
            {
                Debug.unityLogger.LogFormat(logType, format,info.Value);
            }
            DoFinish(true);
        }
    }
}