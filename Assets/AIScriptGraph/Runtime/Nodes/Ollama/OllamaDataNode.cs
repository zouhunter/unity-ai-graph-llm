using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UFrame.NodeGraph;

using UnityEngine;
using UnityEngine.Networking;

namespace AIScripting.Ollama
{
    [CustomNode("OllamaData", 0, Define.GROUP)]
    public class OllamaDataNode : ScriptNodeBase
    {
        [Tooltip("会话缓存")]
        public Ref<List<SendData>> m_DataList;
        public Ref<SendData> data;
        public bool append;

        protected override void OnProcess()
        {
            if (append)
            {
                m_DataList.Value.Add(data.Value);
            }
            else
            {
                m_DataList.Value.Clear();
                m_DataList.Value.Add(data.Value);
            }
            DoFinish(true);
        }
    }
}
