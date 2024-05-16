using System.Collections.Generic;
using System.Linq;
using UFrame.NodeGraph.DataModel;
using System.Reflection;
using UnityEngine;
using System;

namespace AIScripting
{
    public abstract class ScriptNodeBase : Node, IScriptGraphNode
    {
        protected AIScriptGraph Owner { get; private set; }
        [SerializeField, HideInInspector]
        protected int _inportNum = 1;
        [SerializeField, HideInInspector]
        protected int _outportNum = 1;
        protected NodeData _data;
        public int InPortNum => _inportNum;
        public int OutPortNum => _outportNum;
        public virtual int Style => 1;

        protected AsyncOp _asyncOp;
        public float progress => _asyncOp != null ? _asyncOp.progress : 0;
        public Status status { get; protected set; }
        [SerializeField]
        protected string _title;

        public override string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_title))
                    _title = GetType().Name;
                return _title;
            }
        }

        protected virtual int GetInCount(int nodeIndex) => int.MaxValue;
        protected virtual int GetOutCount(int nodeIndex) => int.MaxValue;

        public override void Initialize(NodeData data)
        {
            base.Initialize(data);
            this._data = data;
            while (data.InputPoints.Count < InPortNum)
            {
                var num = data.InputPoints.Count + 1;
                data.AddInputPoint("i", "->", GetInCount(num -1));
            }
            while (data.InputPoints.Count > InPortNum)
            {
                data.InputPoints.RemoveAt(data.InputPoints.Count - 1);
            }
            for (int i = 0; i < data.InputPoints.Count; i++)
            {
                data.InputPoints[i].RefreshInfo("i", "->", GetInCount(i));
            }
            while (data.OutputPoints.Count < OutPortNum)
            {
                var num = data.OutputPoints.Count + 1;
                data.AddOutputPoint("o", "->", GetOutCount(num-1));
            }
            while (data.OutputPoints.Count > OutPortNum)
            {
                data.OutputPoints.RemoveAt(data.OutputPoints.Count - 1);
            }
            for (int i = 0; i < data.OutputPoints.Count; i++)
            {
                data.OutputPoints[i].RefreshInfo("o", "->", GetOutCount(i));
            }
        }

        [ContextMenu("Add In Port")]
        public virtual void AddInPort()
        {
            _inportNum++;
            Initialize(_data);
        }

        [ContextMenu("Del In Port")]
        public virtual void DelInPort()
        {
            _inportNum--;
            Initialize(_data);
        }

        [ContextMenu("Add Out Port")]
        public virtual void AddOutPort()
        {
            _outportNum++;
            Initialize(_data);
        }

        [ContextMenu("Del Out Port")]
        public virtual void DelOutPort()
        {
            _outportNum--;
            Initialize(_data);
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
            Debug.Log("node finish:" + Title + "," + success);
            status = success ? Status.Success : Status.Failure;
            _asyncOp?.SetFinish();
        }

        public virtual AsyncOp Run(string id = default)
        {
            BindingRefVars();
            _asyncOp = new AsyncOp();
            _asyncOp.Id = id;
            status = Status.Running;
            UnityEngine.Debug.Log("node start process:" + Title);
            OnProcess();
            return _asyncOp;
        }

        protected virtual void OnCancel() { }

        public void Cancel()
        {
            if (status == Status.Running)
            {
                status = Status.Failure;
                OnCancel();
                _asyncOp.SetFinish();
            }
        }
    }
}
