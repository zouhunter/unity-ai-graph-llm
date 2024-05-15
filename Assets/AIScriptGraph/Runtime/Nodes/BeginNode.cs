using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;
using UnityEngine;

namespace AIScripting
{
    [CustomNode("Begin",group:"AIScripting")]
    public class BeginNode : ScriptNodeBase
    {
        protected override int InCount => int.MaxValue;
        protected override int OutCount => int.MaxValue;
        public override int Style => 0;
        protected override void OnProcess()
        {
            DoFinish(true);
        }
    }
}