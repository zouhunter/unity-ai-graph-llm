using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UFrame.NodeGraph.DataModel;

namespace AIScripting
{
    public class AIScriptGraph : NodeGraphObj, IScriptGraphNode, IVariableProvider
    {
        private AsyncOp _operate;
        private Status _status;
        private Dictionary<IScriptGraphNode, List<IScriptGraphNode>> _parentNodeMap = new();
        private Dictionary<IScriptGraphNode, List<IScriptGraphNode>> _subNodeMap = new();
        private HashSet<IScriptGraphNode> _inExecuteNodes = new();
        public Dictionary<Type, FieldInfo[]> fieldMap = new();
        public string Name => name;
        public Status status => _status;
        public float progress { get; private set; }
        private AIScriptGraph _runingGraph;
        private VariableProvider _variableProvider = new VariableProvider();
        private List<LitCoroutine> _coroutines = new List<LitCoroutine>();
        public void ResetGraph(AIScriptGraph graph)
        {
            _runingGraph = graph;
            _variableProvider = graph._variableProvider;
        }

        #region Variables
        public void SetVariable(string name, Variable variable)
        {
            _variableProvider?.SetVariable(name, variable);
        }
        public Variable<T> GetVariable<T>(string name, bool crateIfNotExits)
        {
            return _variableProvider.GetVariable<T>(name, crateIfNotExits);
        }
        public T GetVariableValue<T>(string name)
        {
            return _variableProvider.GetVariableValue<T>(name);
        }
        public void SetVariableValue<T>(string name,T data)
        {
            _variableProvider.SetVariableValue(name, data);
        }
        #endregion Variables

        public AsyncOp Run()
        {
            if (_status != Status.None && _operate != null)
                return _operate;
            _operate = new AsyncOp(this);
            _status = Status.Running;
            StartUp();
            return _operate;
        }


        private void StartUp()
        {
#if UNITY_EDITOR
            if(!UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorApplication.update += this.Update;
#endif
            foreach (var node in Nodes)
            {
                if (node.Object is ScriptNodeBase aiNode)
                {
                    aiNode.ResetGraph(_runingGraph ?? this);

                    if (node.InputPoints.Count > 0)
                    {
                        node.InputPoints.ForEach(p =>
                        {
                            var connections = Connections.FindAll(x => x.ToNodeId == node.Id);
                            connections.ForEach(c =>
                            {
                                var fromNodeInfo = Nodes.Find(x => x.Id == c.FromNodeId);
                                if (fromNodeInfo.Object is IScriptGraphNode fromNode)
                                {
                                    if (!_parentNodeMap.TryGetValue(aiNode, out var parentNodes))
                                    {
                                        parentNodes = new List<IScriptGraphNode>();
                                        _parentNodeMap[aiNode] = parentNodes;
                                    }
                                    parentNodes.Add(fromNode);

                                    if (!_subNodeMap.TryGetValue(fromNode, out var subNodes))
                                    {
                                        subNodes = new List<IScriptGraphNode>();
                                        _subNodeMap[fromNode] = subNodes;
                                    }
                                    subNodes.Add(aiNode);
                                }
                            });
                        });
                    }
                }
            }
            var beginNodes = Nodes.FindAll(x => x.Object.GetType() == typeof(BeginNode));
            if (beginNodes != null && beginNodes.Count > 0)
            {
                foreach (var beginNode in beginNodes)
                {
                    TryRunNode(beginNode.Object as ScriptNodeBase);
                }
            }
            else
            {
                _status = Status.Failure;
                _operate.SetFinish();
            }
        }

        protected void TryRunNode(IScriptGraphNode node)
        {
            bool parentFinished = true;
            if (_parentNodeMap.TryGetValue(node, out var parentNodes) && parentNodes.Count > 0)
            {
                foreach (var parentNode in parentNodes)
                {
                    if (parentNode.status == Status.None || parentNode.status == Status.Running)
                    {
                        parentFinished = false;
                        TryRunNode(parentNode);
                    }
                }
            }

            if (parentFinished && node.status == Status.None)
            {
                UnityEngine.Debug.Log("node start:" + node.Name);
                _inExecuteNodes.Add(node);
                var operate = node.Run();
                operate.RegistProgress(OnProgressNode);
                operate.RegistComplete(OnFinishNode);
            }
        }

        private void OnProgressNode(IScriptGraphNode node)
        {
            UnityEngine.Debug.Log("node progress:" + node.Name + "," + node.progress);
        }

        protected void OnFinishNode(IScriptGraphNode node)
        {
            if (_status != Status.Running)
                return;

            _inExecuteNodes.Remove(node);
            if (_subNodeMap.TryGetValue(node, out var subNodes))
            {
                foreach (var subNode in subNodes)
                {
                    TryRunNode(subNode);
                }
            }

            if (_inExecuteNodes.Count == 0)
            {
                _status = Status.Success;
                OnFinished();
            }
        }

        public void Cancel()
        {
            _status = Status.Failure;
            foreach (var inExecute in _inExecuteNodes)
                inExecute.Cancel();
            _inExecuteNodes.Clear();
            _operate?.SetFinish();
            OnFinished();
        }

        protected void OnFinished()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorApplication.update -= this.Update;
#endif
        }

        void OnDestroy()
        {
            Cancel();
        }

        public void Update()
        {
            if (_coroutines != null && _coroutines.Count > 0)
            {
                for (int i = _coroutines.Count - 1; i >= 0; i--)
                {
                    var coroutine = _coroutines[i];
                    coroutine.Update();
                    if (coroutine.IsDone)
                    {
                        _coroutines.RemoveAt(i);
                    }
                }
            }
        }

        public LitCoroutine StartCoroutine(IEnumerator enumerator)
        {
            var litCoroutine = new LitCoroutine(enumerator,this);
            _coroutines.Add(litCoroutine);
            return litCoroutine;
        }
    }
}
