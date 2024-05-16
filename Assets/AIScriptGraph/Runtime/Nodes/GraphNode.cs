using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEngine;

namespace AIScripting
{
    [CustomNode("Graph", group: Define.GROUP)]
    public class GraphNode : ScriptNodeBase
    {
        public AIScriptGraph graph;
        public override int Style => 2;

        public override void ResetGraph(AIScriptGraph graph)
        {
            base.ResetGraph(graph);
            this.graph.ResetGraph(graph);
        }

        protected override void OnProcess()
        {
            var op = graph.Run();
            op.RegistComplete(OnFinish);
        }

        private void OnFinish(string id)
        {
            _asyncOp?.SetFinish();
        }
    }
}