using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;
using UnityEngine;

namespace AIScripting
{
    [CustomNode("Debug",group:"AIScripting")]
    public class DebugNode : ScriptAINodeBase
    {
        [InPort]
        public Ref<string> info;
        public LogType logType;
        protected override int InCount => int.MaxValue;
    }
}