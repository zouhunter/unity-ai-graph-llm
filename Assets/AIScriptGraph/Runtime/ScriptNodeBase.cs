using System.Collections.Generic;
using System.Linq;
using UFrame.NodeGraph.DataModel;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

namespace AIScripting
{
    public abstract class ScriptNodeBase : Node, IScriptGraphNode
    {
        [SerializeField]
        private string _title;
        protected AIScriptGraph Owner { get; private set; }
        protected virtual int InCount => int.MaxValue;
        protected virtual int OutCount => int.MaxValue;
        public virtual int Style => 0;

        protected AsyncOp _asyncOp;
        public float progress => _asyncOp != null ? _asyncOp.progress : 0;
        public Status status { get; protected set; }
        public override string Title
        {
            get
            {
                if(string.IsNullOrEmpty(_title))
                    _title = GetType().Name;
                return _title;
            }
        }

        public override void Initialize(NodeData data)
        {
            base.Initialize(data);
            if (InCount > 0 && data.InputPoints.Count == 0)
            {
                data.AddInputPoint("i", "->", InCount);
            }
            else if(data.InputPoints.Count > 0)
            {
                data.InputPoints[0].RefreshInfo("i", "->", InCount);
            }
            if (OutCount > 0 && data.OutputPoints.Count == 0)
            {
                data.AddOutputPoint("o", "->", OutCount);
            }
            else if (data.OutputPoints.Count > 0)
            {
                data.OutputPoints[0].RefreshInfo("o", "->", OutCount);
            }
        }

        public virtual void ResetGraph(AIScriptGraph graph)
        {
            Owner = graph;
            status = Status.None;
        }

        /// <summary>
        /// 反射获取所有的引用变量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IRef> GetRefVars()
        {
            var type = GetType();
            if (!Owner.fieldMap.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).
                    Where(f => typeof(IRef).IsAssignableFrom(f.FieldType)).ToArray();
                Owner.fieldMap[type] = fields;
            }
            return fields
                .Select(f => f.GetValue(this) as IRef)
                .Where(r => r != null);
        }

        protected void BindingRefVars()
        {
            var refVars = GetRefVars();
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
            Debug.Log("node finish:" + Title + "," +success);
            status = success ? Status.Success : Status.Failure;
            _asyncOp?.SetFinish();
        }

        public virtual AsyncOp Run()
        {
            BindingRefVars();
            _asyncOp = new AsyncOp(this);
            status = Status.Running;
            UnityEngine.Debug.Log("node start process:" + Title);
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
