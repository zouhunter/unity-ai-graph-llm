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
        private Dictionary<string, List<string>> _parentNodeMap = new();
        private Dictionary<string, List<string>> _subNodeMap = new();
        private Dictionary<string, IScriptGraphNode> _nodeMap = new();
        private Dictionary<string, Dictionary<string, NodeConnection>> _connectionMap = new();
        private HashSet<IScriptGraphNode> _inExecuteNodes = new();
        private Queue<string> _nextExecuteNodes = new();
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
        public void SetVariableValue<T>(string name, T data)
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
            _operate = new AsyncOp();
            _status = Status.Running;
            StartUp();
            return _operate;
        }


        private void StartUp()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorApplication.update += this.Update;
#endif
            _parentNodeMap.Clear();
            _subNodeMap.Clear();
            _connectionMap.Clear();
            foreach (var node in Nodes)
            {
                if (node.Object is ScriptNodeBase aiNode)
                {
                    aiNode.ResetGraph(_runingGraph ?? this);
                    _nodeMap[node.Id] = aiNode;
                    if (node.OutputPoints.Count > 0)
                    {
                        node.OutputPoints.ForEach(p =>
                        {
                            var connections = Connections.FindAll(x => x.FromNodeId == node.Id);
                            connections.Sort((x, y) =>
                            {
                                return -(x.Object is NodeConnection portConnection ? portConnection.priority : 0)
                                 + (y.Object is NodeConnection portConnection2 ? portConnection2.priority : 0);
                            });

                            connections.ForEach(c =>
                            {
                                if (c.Object is NodeConnection portConnection && portConnection.disable)
                                    return;

                                var toNodeInfo = Nodes.Find(x => x.Id == c.ToNodeId);
                                if (toNodeInfo.Object is IScriptGraphNode toNode)
                                {
                                    _nodeMap[toNodeInfo.Id] = toNode;
                                    if (!_parentNodeMap.TryGetValue(toNodeInfo.Id, out var parentNodes))
                                    {
                                        parentNodes = new List<string>();
                                        _parentNodeMap[toNodeInfo.Id] = parentNodes;
                                    }
                                    parentNodes.Add(node.Id);
                                    if (!_subNodeMap.TryGetValue(node.Id, out var subNodes))
                                    {
                                        subNodes = new List<string>();
                                        _subNodeMap[node.Id] = subNodes;
                                    }
                                    subNodes.Add(toNodeInfo.Id);
                                    SetConnection(node.Id, toNodeInfo.Id, c.Object as NodeConnection);
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
                    TryRunNode(beginNode.Id);
                }
            }
            else
            {
                _status = Status.Failure;
                _operate.SetFinish();
            }
        }

        private void SetConnection(string parentId, string childId, NodeConnection connection)
        {
            if (!_connectionMap.TryGetValue(parentId, out var childMap))
                childMap = _connectionMap[parentId] = new Dictionary<string, NodeConnection>();
            childMap[childId] = connection;
        }

        private NodeConnection GetConnection(string parentId, string childId)
        {
            if (_connectionMap.TryGetValue(parentId, out var childMap) && childMap.TryGetValue(childId, out var connection))
                return connection;
            return null;
        }

        /// <summary>
        /// 检查是否有循环引用
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        protected bool CheckStackOverFlow(string nodeId, string parentId)
        {
            if (_subNodeMap.TryGetValue(nodeId, out var childIds))
            {
                foreach (var childId in childIds)
                {
                    if (childId == parentId)
                    {
                        return true;
                    }
                    else
                    {
                        var match = CheckStackOverFlow(childId, nodeId);
                        if (match)
                            return true;
                        else
                        {
                            match = CheckStackOverFlow(childId, parentId);
                            if (match)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        protected void TryRunNode(string nodeId)
        {
            bool parentFinished = true;
            if (_parentNodeMap.TryGetValue(nodeId, out var parentNodes) && parentNodes.Count > 0)
            {
                foreach (var parentNodeId in parentNodes)
                {
                    if (CheckStackOverFlow(nodeId, parentNodeId))
                        continue;

                    var connection = GetConnection(parentNodeId, nodeId);

                    if (!connection.Pass())
                        continue;

                    var parentNode = _nodeMap[parentNodeId];
                    if (parentNode.status == Status.None || parentNode.status == Status.Running)
                    {
                        parentFinished = false;
                        TryRunNode(parentNodeId);
                    }
                }
            }

            if (parentFinished)
            {
                var node = _nodeMap[nodeId];
                _nextExecuteNodes.Enqueue(nodeId);
                _inExecuteNodes.Add(node);
            }
        }


        private void OnProgressNode(string nodeId)
        {
            var node = _nodeMap[nodeId];
            UnityEngine.Debug.Log("node progress:" + node.Title + "," + node.progress);
        }

        protected void OnFinishNode(string nodeId)
        {
            if (_status != Status.Running)
                return;
            var node = _nodeMap[nodeId];
            _inExecuteNodes.Remove(node);
            if (_subNodeMap.TryGetValue(nodeId, out var subNodes))
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
            _nextExecuteNodes.Clear();
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

            if(_nextExecuteNodes.Count > 0)
            {
                var nodeId = _nextExecuteNodes.Dequeue();
                var node = _nodeMap[nodeId];
                var operate = node.Run();
                operate.Id = nodeId;
                operate.RegistProgress(OnProgressNode);
                operate.RegistComplete(OnFinishNode);
            }
        }

        public LitCoroutine StartCoroutine(IEnumerator enumerator)
        {
            var litCoroutine = new LitCoroutine(enumerator, this);
            _coroutines.Add(litCoroutine);
            return litCoroutine;
        }
    }
}
