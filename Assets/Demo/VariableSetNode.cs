using AIScripting;

using System.Collections;
using System.Collections.Generic;

using UFrame.NodeGraph;

using UnityEngine;

[CustomNode("VariableSetNode",0,"AIScripting")]
public class VariableSetNode : VariableInitNode
{
    public Ref<string> ollama_url = new Ref<string>() { Value = "http://localhost:8081/api/chat" };
}
