using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;
using UnityEngine;

namespace AIScripting
{
    [CustomNode("Begin",group:"AIScripting")]
    public class BeginNode : ScriptAINodeBase
    {
        public Ref<string> text;
        protected override int OutCount => int.MaxValue;
    }
}