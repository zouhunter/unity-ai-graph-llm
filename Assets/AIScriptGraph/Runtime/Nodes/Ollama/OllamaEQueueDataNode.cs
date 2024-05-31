using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Ollama
{
    [CustomNode("OllamaData", 0, Define.GROUP)]
    public class OllamaEQueueDataNode : ScriptNodeBase
    {
        [Tooltip("会话来源")]
        public Ref<SendData> fromData;
        [Tooltip("会话缓存")]
        public Ref<List<SendData>> targetSendList;
        public bool append;

        protected override void OnProcess()
        {
            if (append)
            {
                targetSendList.Value.Add(fromData.Value);
            }
            else
            {
                targetSendList.Value.Clear();
                targetSendList.Value.Add(fromData.Value);
            }
            DoFinish(true);
        }
    }
}
