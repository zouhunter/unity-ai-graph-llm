using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

namespace AIScripting
{
    public class AIScriptGraph : NodeGraphObj, IVariableProvider, IScriptGraphNode
    {
        private AsyncOp _operate;
        private Status _status;
        private Dictionary<Type, IEnumerable<FieldInfo>> _fieldMap = new ();
        private Dictionary<IScriptGraphNode, List<IScriptGraphNode>> _parentNodeMap = new ();
        private Dictionary<IScriptGraphNode, List<IScriptGraphNode>> _subNodeMap = new ();
        private Queue<IScriptGraphNode> _inExecuteNodes = new();
        public string Name => name;
        public Status status => _status;
        public float progress { get; private set; }

        #region Variables
        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();

        public Variable GetVariable(string name)
        {
            _variables.TryGetValue(name, out var variable);
            return variable;
        }
        public Variable<T> GetVariable<T>(string name, bool createIfNotExits = true)
        {
            if (_variables.TryGetValue(name, out var variable) && variable is Variable<T> genVariable)
            {
                return genVariable;
            }
            else if (createIfNotExits)
            {
                var newVariable = new Variable<T>();
                _variables[name] = newVariable;
                return newVariable;
            }
            return null;
        }
        public T GetVariableValue<T>(string name)
        {
            if (_variables.TryGetValue(name, out var variable) && variable.GetValue() is T value)
            {
                return value;
            }
            return default(T);
        }
        public bool TryGetVariable<T>(string name, out Variable<T> variable)
        {
            if (_variables.TryGetValue(name, out var variableObj) && variableObj is Variable<T> genVariable && genVariable != null)
            {
                variable = genVariable;
                return true;
            }
            variable = null;
            return false;
        }
        public bool TryGetVariable(string name, out Variable variable)
        {
            return _variables.TryGetValue(name, out variable);
        }
        public void SetVariable(string name, Variable variable)
        {
            _variables[name] = variable;
        }
        public bool SetVariableValue(string name, object data)
        {
            if (_variables.TryGetValue(name, out var variable))
            {
                variable.SetValue(data);
                return true;
            }
            return false;
        }
        public void SetVariableValue<T>(string name, T data)
        {
            var variable = GetVariable<T>(name, true);
            variable.Value = data;
        }
        #endregion Variables

        public void ClearCondition()
        {
            _variables.Clear();
        }

        public void Binding(AIScriptGraph graph)
        {
            //TDOO 兼容
        }

        public AsyncOp Run()
        {
            if(_status != Status.None && _operate != null)
                return _operate;
            _operate = new AsyncOp(this);
            _status = Status.Running;
            StartUp();
            return _operate;
        }

        private void StartUp()
        {
            foreach (var node in Nodes)
            {
                if (node.Object is ScriptNodeBase aiNode)
                {
                    aiNode.Binding(this);

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

                                    if(!_subNodeMap.TryGetValue(fromNode, out var subNodes))
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
            var beginNode = Nodes.Find(x => x.Object.GetType() == typeof(BeginNode));
            if (beginNode != null)
            {
                TryRunNode(beginNode.Object as ScriptNodeBase);
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
                    if(parentNode.status == Status.None || parentNode.status == Status.Running)
                    {
                        parentFinished = false;
                        TryRunNode(parentNode);
                    }
                }
            }

            if(parentFinished && node.status == Status.None)
            {
                UnityEngine.Debug.Log("run node:" + node.Name);
                _inExecuteNodes.Enqueue(node);
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
            if(_subNodeMap.TryGetValue(node,out var subNodes))
            {
                foreach (var subNode in subNodes)
                {
                    TryRunNode(subNode);
                }
            }
            if(_inExecuteNodes.Count == 0)
            {
                _status = Status.Success;
                _operate.SetFinish();
            }
        }

        public void Cancel()
        {
            _status = Status.Failure;
            foreach (var inExecute in _inExecuteNodes)
                inExecute.Cancel();
            _operate.SetFinish();
        }
        /// <summary>
        /// 反射获取所有的引用变量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<FieldInfo> GetTypeRefs(Type type)
        {
            if (!_fieldMap.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(f => typeof(IRef).IsAssignableFrom(f.FieldType));
                _fieldMap[type] = fields;
            }
            return fields;
        }

        void OnDestroy()
        {
            Cancel();
        }
    }
}
