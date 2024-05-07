using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEngine;
using UnityEngine.Networking;

namespace AIScripting
{
    public abstract class ScriptAINodeBase : Node
    {
        protected AIScriptGraph Owner { get; private set; }
        protected virtual int InCount =>0;
        protected virtual int OutCount =>0;
        protected AsyncOp _asyncOp;
        public int progress => _asyncOp != null ? _asyncOp.progress : 0;
        public NodeStatus status { get; private set; }

        public override void Initialize(NodeData data)
        {
            base.Initialize(data);
            if(InCount > 0 && data.InputPoints.Count == 0)
            {
                data.AddInputPoint("i", "->", InCount);
            }
            if(OutCount > 0 && data.OutputPoints.Count == 0)
            {
                data.AddOutputPoint("o", "->", OutCount);
            }
        }

        internal void Binding(AIScriptGraph graph)
        {
            Owner = graph;
            status = NodeStatus.None;
            BindingRefVars(GetRefVars());
        }

        protected virtual IEnumerable<IRef> GetRefVars()
        {
            return Owner.GetTypeRefs(GetType())
                .Select(f => f.GetValue(this) as IRef)
                .Where(r => r != null);
        }

        protected void BindingRefVars(IEnumerable<IRef> refVars)
        {
            if (refVars != null)
            {
                foreach (var refVar in refVars)
                {
                    refVar?.Binding(Owner);
                }
            }
        }

        internal virtual AsyncOp Run()
        {
            _asyncOp = new AsyncOp(this);
            status = NodeStatus.Running;
            return _asyncOp;
        }
    }
}
