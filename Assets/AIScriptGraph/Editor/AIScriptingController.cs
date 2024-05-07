using AIScripting;

using System.Collections;
using System.Collections.Generic;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEditor.MemoryProfiler;

using UnityEngine;

namespace AIScripting
{
    public class AIScriptingController : NodeGraphController
    {
        public override string Group => "AIScripting";

        public override NodeGraphObj CreateNodeGraphObject()
        {
            return new AIScriptGraph();
        }

        public override NodeView CreateDefaultNodeView()
        {
            return new ScriptingNodeView();
        }

        public override void BuildFromGraph(NodeGraphObj graph)
        {
            base.BuildFromGraph(graph);
        }

        public override ConnectionGUI CreateConnection(string type, ConnectionPointData output, ConnectionPointData input)
        {
            return new ConnectionGUI(
               new ConnectionData(type, new PortConnection(type), output, input),
               output,
               input
           );
        }
    }
}