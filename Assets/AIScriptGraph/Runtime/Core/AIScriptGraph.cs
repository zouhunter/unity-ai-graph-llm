using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<string, Dictionary<string, List<NodeConnection>>> _connectionMap = new();
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
        private List<AIScriptGraph> _subGraphs = new List<AIScriptGraph>();
        private HashSet<string> _overflowSet = new HashSet<string>();
        public void ResetGraph(AIScriptGraph graph)
        {
            _runingGraph = graph;
            _variableProvider = graph._variableProvider;
            _eventProvider = graph._eventProvider;
            _coroutines = graph._coroutines;
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
        public Variable GetVariable(string name)
        {
            return _variableProvider.GetVariable(name);
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

        public AsyncOp Run(string id = default)
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
            if (!UnityEditor.EditorApplication.isPlaying && _runingGraph == null)
                UnityEditor.EditorApplication.update += this.Update;
#endif
            _subGraphs.Clear();
            _parentNodeMap.Clear();
            _subNodeMap.Clear();
            _connectionMap.Clear();
            foreach (var node in Nodes)
            {
                if (node.Object is ScriptNodeBase aiNode)
                {
                    if (!aiNode.enable)
                        continue;
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
                                if (toNodeInfo.Object is ScriptNodeBase toNode)
                                {
                                    if (!toNode.Enable)
                                        return;

                                    _nodeMap[toNodeInfo.Id] = toNode;
                                    if (!_parentNodeMap.TryGetValue(toNodeInfo.Id, out var parentNodes))
                                    {
                                        parentNodes = new List<string>();
                                        _parentNodeMap[toNodeInfo.Id] = parentNodes;
                                    }
                                    if(!parentNodes.Contains(node.Id))
                                    {
                                        parentNodes.Add(node.Id);
                                    }
                                    if (!_subNodeMap.TryGetValue(node.Id, out var subNodes))
                                    {
                                        subNodes = new List<string>();
                                        _subNodeMap[node.Id] = subNodes;
                                    }
                                    if(!subNodes.Contains(toNodeInfo.Id))
                                    {
                                        subNodes.Add(toNodeInfo.Id);
                                    }
                                    AddConnection(node.Id, toNodeInfo.Id, c.Object as NodeConnection);
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
                    if (!(beginNode.Object as BeginNode).enable)
                        continue;
                    TryRunNode(beginNode.Id);
                }
            }
            else
            {
                _status = Status.Failure;
                _operate.SetFinish();
            }
        }

        private void AddConnection(string parentId, string childId, NodeConnection connection)
        {
            if (!_connectionMap.TryGetValue(parentId, out var childMap))
            {
                childMap = _connectionMap[parentId] = new Dictionary<string, List<NodeConnection>>();
            }
            if(!childMap.TryGetValue(childId,out var connections))
            {
                connections = childMap[childId] = new List<NodeConnection>();
            }
            if(!connections.Contains(connection))
            {
                connections.Add(connection);
            }
        }

        private bool GetConnectionPass(string parentId, string childId)
        {
            if (_connectionMap.TryGetValue(parentId, out var childMap) && childMap.TryGetValue(childId, out var connection))
            {
                foreach (var item in connection)
                {
                    if (item.Pass(this))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查是否有循环引用
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        protected bool CheckStackOverFlow(string nodeId, string parentId, HashSet<string> content)
        {
            if (content.Contains(nodeId))
                return true;
            content.Add(nodeId);

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
                        var match = CheckStackOverFlow(childId, parentId, content);
                        if (match)
                            return true;
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
                    _overflowSet.Clear();
                    if (CheckStackOverFlow(nodeId, parentNodeId, _overflowSet))
                        continue;

                    var connectionPass = GetConnectionPass(parentNodeId, nodeId);

                    if (!connectionPass)
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
                if(node.status != Status.Running)
                {
                    _nextExecuteNodes.Enqueue(nodeId);
                    _inExecuteNodes.Add(node);
                }
            }
        }


        private void OnProgressNode(string nodeId)
        {
            var node = _nodeMap[nodeId];
            UnityEngine.Debug.Log(node.Title + ", progress" + node.progress);
        }

        /// <summary>
        /// 节点执行结束
        /// </summary>
        /// <param name="nodeId"></param>
        protected void OnFinishNode(string nodeId)
        {
            if (_status != Status.Running)
                return;

            var node = _nodeMap[nodeId];
            _inExecuteNodes.Remove(node);
            if (node is GraphNode graphNode && graphNode.graph)
                _subGraphs.Remove(graphNode.graph);
            if (_subNodeMap.TryGetValue(nodeId, out var subNodes))
            {
                foreach (var subNode in subNodes)
                {
                    var pass = GetConnectionPass(nodeId,subNode);
                    if (!pass)
                        continue;

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
            UnityEngine.Debug.Log(name + " graph finished!");
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying && _runingGraph == null)
                UnityEditor.EditorApplication.update -= this.Update;
#endif
        }

        void OnDestroy()
        {
            Cancel();
        }

        public void ResetGraph()
        {
            _operate = null;
            _runingGraph = null;
            progress = 0;
            _status = Status.None;
            _parentNodeMap = new Dictionary<string, List<string>>();
            _subNodeMap = new Dictionary<string, List<string>>();
            _inExecuteNodes = new HashSet<IScriptGraphNode>();
            _nextExecuteNodes = new Queue<string>();
            _variableProvider = new VariableProvider();
            _eventProvider = new EventProvider();
        }

        public void Update()
        {
            try
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

                if (_nextExecuteNodes.Count > 0)
                {
                    var nodeId = _nextExecuteNodes.Dequeue();
                    var node = _nodeMap[nodeId];
                    var operate = node.Run(nodeId);
                    operate.RegistProgress(OnProgressNode);
                    operate.RegistComplete(OnFinishNode);
                    if (node is GraphNode graphNode && graphNode.graph)
                        _subGraphs.Add(graphNode.graph);
                }

                foreach (var subGraph in _subGraphs)
                {
                    subGraph?.Update();
                }

            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
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
