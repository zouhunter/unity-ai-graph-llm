using System.Collections.Generic;
using System.Linq;
using UFrame.NodeGraph.DataModel;

namespace AIScripting
{
    public abstract class ScriptNodeBase : Node, IScriptGraphNode
    {
        protected AIScriptGraph Owner { get; private set; }
        protected virtual int InCount => 0;
        protected virtual int OutCount => 0;
        public virtual int Style => 0;

        protected AsyncOp _asyncOp;
        public float progress => _asyncOp != null ? _asyncOp.progress : 0;
        public Status status { get; protected set; }
        public string Name => name;

        public override void Initialize(NodeData data)
        {
            base.Initialize(data);
            if (InCount > 0 && data.InputPoints.Count == 0)
            {
                data.AddInputPoint("i", "->", InCount);
            }
            if (OutCount > 0 && data.OutputPoints.Count == 0)
            {
                data.AddOutputPoint("o", "->", OutCount);
            }
        }

        public void Binding(AIScriptGraph graph)
        {
            Owner = graph;
            status = Status.None;
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

        protected abstract void OnProcess();

        protected virtual void DoFinish(bool success = true)
        {
            status = success ? Status.Success : Status.Failure;
            _asyncOp?.SetFinish();
        }

        public AsyncOp Run()
        {
            _asyncOp = new AsyncOp(this);
            status = Status.Running;
            OnProcess();
            return _asyncOp;
        }

        protected virtual void OnCancel() { }

        public void Cancel()
        {
            if(status == Status.Running)
            {
                status = Status.Failure;
                OnCancel();
                _asyncOp.SetFinish();
            }
        }
    }
}
