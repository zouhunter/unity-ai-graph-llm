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
        public string Title => name;
        public Status status => _status;
        public float progress { get; private set; }
        private AIScriptGraph _runingGraph;
        private VariableProvider _variableProvider = new VariableProvider();
        private List<LitCoroutine> _coroutines = new List<LitCoroutine>();
        private EventProvider _eventProvider = new EventProvider();
        public void ResetGraph(AIScriptGraph graph)
        {
            _runingGraph = graph;
            _variableProvider = graph._variableProvider;
            _eventProvider = graph._eventProvider;
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

        #region Events
        public void RegistEvent(string eventKey, Action<object> callback)
        {
            _eventProvider?.RegistEvent(eventKey, callback);
        }
        public void RemoveEvent(string eventKey, Action<object> callback)
        {
            _eventProvider?.RemoveEvent(eventKey, callback);
        }
        public void SendEvent(string eventKey, object arg = null)
        {
            _eventProvider?.SendEvent(eventKey, arg);
        }
        #endregion

        public AsyncOp Run()
        {
            if (_status == Status.Running && _operate != null)
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
                    if(node.OutputPoints.Count > 0)
                    {
                        node.OutputPoints.ForEach(p => {
                            var connections = Connections.FindAll(x => x.FromNodeId == node.Id);
                            connections.Sort((x, y) => {
                                return - (x.Object is PortConnection portConnection ? portConnection.priority : 0)
                                 + (y.Object is PortConnection portConnection2 ? portConnection2.priority : 0);
                            });

                            connections.ForEach(c =>
                            {
                                if (c.Object is PortConnection portConnection && portConnection.disable)
                                    return;

                                var toNodeInfo = Nodes.Find(x => x.Id == c.ToNodeId);
                                if (toNodeInfo.Object is IScriptGraphNode toNode)
                                {
                                    if (!_parentNodeMap.TryGetValue(toNode, out var parentNodes))
                                    {
                                        parentNodes = new List<IScriptGraphNode>();
                                        _parentNodeMap[toNode] = parentNodes;
                                    }
                                    parentNodes.Add(aiNode);
                                    if (!_subNodeMap.TryGetValue(aiNode, out var subNodes))
                                    {
                                        subNodes = new List<IScriptGraphNode>();
                                        _subNodeMap[aiNode] = subNodes;
                                    }
                                    subNodes.Add(toNode);
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
                _inExecuteNodes.Add(node);
                var operate = node.Run();
                operate.RegistProgress(OnProgressNode);
                operate.RegistComplete(OnFinishNode);
            }
        }

        private void OnProgressNode(IScriptGraphNode node)
        {
            UnityEngine.Debug.Log("node progress:" + node.Title + "," + node.progress);
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
            UnityEngine.Debug.Log("graph finished!");
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorApplication.update -= this.Update;
#endif
        }

        void OnDestroy()
        {
            Cancel();
        }

        public void Clean()
        {
            _operate = null;
            _status = Status.None;
            _parentNodeMap.Clear();
            _subNodeMap.Clear();
            _inExecuteNodes.Clear();
            _variableProvider = new VariableProvider();
            _eventProvider = new EventProvider();
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
