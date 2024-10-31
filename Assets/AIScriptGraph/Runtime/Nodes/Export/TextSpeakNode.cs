using System;
using System.Buffers.Text;
using UFrame.NodeGraph;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace AIScripting.Export
{
    [CustomNode("TextSpeak", group: Define.GROUP)]
    public class TextSpeakNode : ScriptNodeBase
    {
        [Tooltip("info text")]
        public Ref<string> textInfo;
        public string pythonFolder = "Assets\\AIScriptGraph\\Runtime\\Python";
        private string m_Hypotheses;
        private PythonExecuter _executer;
        public string eventName = "uframe_receive_message";
        private StringBuilder _messageQueue;
        private bool _finished = false;
        private bool _isSpeaking = false;
        public override void ResetGraph(AIScriptGraph graph)
        {
            base.ResetGraph(graph);
            graph.RemoveEvent(eventName, OnRecvMessage);
            graph.RegistEvent(eventName, OnRecvMessage);
            _messageQueue = new StringBuilder();
            _finished = false;
            _isSpeaking = false;

            if (_executer == null)
                _executer = new PythonExecuter(pythonFolder);
        }

        private void OnRecvMessage(object message)
        {
            string text = "";
            if (message is string)
            {
                text = message.ToString();
            }
            else if (message is ReceiveData recv)
            {
                text = recv.message.content;
            }
            _messageQueue.Append(text);
            if(!_isSpeaking)
            {
                _isSpeaking = true;
                SpeakAsync();
            }
        }

        protected override void OnProcess()
        {
            _finished = true;
        }

        private async void SpeakAsync()
        {
            int stateId = 0;
            while (!_finished || _messageQueue.Length > 0)
            {
                if (_messageQueue.Length > 0)
                {
                    var text = _messageQueue.ToString();
                    if(!string.IsNullOrEmpty(text))
                    {
                        _messageQueue.Clear();
                        stateId = await _executer.RunPythonAsync("speak", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)));
                    }
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
            DoFinish(stateId == 0);
        }
    }
}
