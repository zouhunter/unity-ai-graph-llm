/*-*-* Copyright (c) mateai@wekoi
 * Author: 
 * Creation Date: 2024-05-21
 * Version: 1.0.0
 * Description: 
 *_*/

using UnityEngine;
using UFrame.NodeGraph;
using AIScripting.Debugger;
using System.Text;

namespace AIScripting.Debugger
{
    [CustomNode("DiffShow", group: Define.GROUP)]
    public class DiffShowNode : ScriptNodeBase
    {
        public string eventName = "wekoi_receive_message";
        public string saveFilePath;
        public Ref<string> sourceText;
        public StringBuilder allText { get; private set; }

        public override void ResetGraph(AIScriptGraph graph)
        {
            base.ResetGraph(graph);
            graph.RemoveEvent(eventName, OnRecvMessage);
            graph.RegistEvent(eventName, OnRecvMessage);
            allText = allText?? new StringBuilder();
            allText.Clear();
        }

        private void OnRecvMessage(object message)
        {
            var text = message;
            allText.Append(message);
        }

        protected override void OnProcess()
        {
            DoFinish(true);
        }
    }
}

