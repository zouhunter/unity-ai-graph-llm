using AIScripting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIScripting
{
    public class GraphDirecterBehaviour : MonoBehaviour
    {
        public string beginNode;
        public AIScriptGraph graph;
        public List<BindingInfo> bindings = new List<BindingInfo>();
        public GraphDirector graphDirector;

        public Button sendBtn;
        public InputField inputField;
        public Text resultText;

        private void Awake()
        {
            graphDirector = new GraphDirector(graph);
            graphDirector.Binding(bindings);
            sendBtn.onClick.AddListener(OnSendClick);
            graph.RegistEvent("ollama_receive_message", OnRecvMessage);
        }

        public void OnSendClick()
        {
            resultText.text = "";
            graph.SetVariable("input_text", new Variable<string>() { Value = inputField.text });
            var op = graphDirector.Run();
            op.RegistComplete(OnFinish);
        }

        private void OnRecvMessage(object obj)
        {
            var recvData = obj as OllamaNode.ReceiveData;
            //Debug.Log("ollama_receive_message:" + recvData.message.content);
            resultText.text += recvData.message.content;
        }

        private void OnFinish(IScriptGraphNode obj)
        {
            resultText.text = graph.GetVariableValue<string>("out_put");
        }

        private void Update()
        {
            graphDirector?.Update();
        }
    }
}