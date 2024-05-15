using AIScripting;
using AIScripting.Ollama;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class GraphDirecterBehaviour : MonoBehaviour
{
    [Tooltip("图")]
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
        graph.RegistEvent("wekoi_receive_message", OnRecvMessage);
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
        if (!resultText)
            return;
        if(obj is ReceiveData)
        {
            var recvData = obj as ReceiveData;
            //Debug.Log("ollama_receive_message:" + recvData.message.content);
            resultText.text += recvData.message.content;
        }
        else
        {
            resultText.text += obj.ToString();
        }
    }

    private void OnFinish(IScriptGraphNode obj)
    {
        if (!resultText)
            return;

        resultText.text = graph.GetVariableValue<string>("out_put");
    }

    private void Update()
    {
        graphDirector?.Update();
    }
}
