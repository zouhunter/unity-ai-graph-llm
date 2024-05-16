using AIScripting;

using System.Collections;
using System.Collections.Generic;

using UFrame.NodeGraph;

using UnityEngine;

[CustomNode("Demo.VariableSetNode",0,Define.GROUP)]
public class VariableSetNode : AIScripting.Debugger.VariableInitNode
{
    public Ref<string> ollama_url= new Ref<string>("ollama_url", "http://localhost:8081/api/chat");
    public Ref<string> input_text = new Ref<string>("input_text", "ÄãºÃ");
    public GameObject target;
}
