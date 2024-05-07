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
        }

        public void OnSendClick()
        {
            graph.SetVariable("input_text", new Variable<string>() { Value = inputField.text});
            var op = graphDirector.Run();
            op.RegistComplete(OnFinish);
        }

        private void OnFinish(IScriptGraphNode obj)
        {
            resultText.text = graph.GetVariableValue<string>("out_put");
        }
    }
}