using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using Unity.VisualScripting;

namespace AIScripting
{
    public class AIScriptGraph : NodeGraphObj, IVariableProvider
    {

        private Dictionary<Type, IEnumerable<FieldInfo>> _fieldMap = new Dictionary<Type, IEnumerable<FieldInfo>>();
        private Dictionary<ScriptAINodeBase, List<ScriptAINodeBase>> _parentNodeMap = new Dictionary<ScriptAINodeBase, List<ScriptAINodeBase>>();
        private Dictionary<ScriptAINodeBase, List<ScriptAINodeBase>> _subNodeMap = new Dictionary<ScriptAINodeBase, List<ScriptAINodeBase>>();

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

        public void StartUp(string startNodeName)
        {
            foreach (var node in Nodes)
            {
                if (node.Object is ScriptAINodeBase aiNode)
                {
                    aiNode.Binding(this);

                    if (node.InputPoints.Count > 0)
                    {
                        node.InputPoints.ForEach(p =>
                        {
                            var connections = Connections.FindAll(x => x.ToNodeId == p.Id);
                            connections.ForEach(c =>
                            {
                                var fromNodeInfo = Nodes.Find(x => x.Id == c.FromNodeId);
                                if (fromNodeInfo.Object is ScriptAINodeBase fromNode)
                                {
                                    if (!_parentNodeMap.TryGetValue(aiNode, out var parentNodes))
                                    {
                                        parentNodes = new List<ScriptAINodeBase>();
                                        _parentNodeMap[aiNode] = parentNodes;
                                    }
                                    parentNodes.Add(fromNode);

                                    if(!_subNodeMap.TryGetValue(fromNode, out var subNodes))
                                    {
                                        subNodes = new List<ScriptAINodeBase>();
                                        _subNodeMap[fromNode] = subNodes;
                                    }
                                    subNodes.Add(aiNode);
                                }
                            });
                        });
                    }
                }
            }
            var beginNode = Nodes.Find(x => x.Name == startNodeName);
            if (beginNode != null)
            {
                TryRunNode(beginNode.Object as ScriptAINodeBase);
            }
        }
        protected void TryRunNode(ScriptAINodeBase node)
        {
            bool parentFinished = true;
            if (_parentNodeMap.TryGetValue(node, out var parentNodes) && parentNodes.Count > 0)
            {
                foreach (var parentNode in parentNodes)
                {
                    if(parentNode.status == NodeStatus.None || parentNode.status == NodeStatus.Running)
                    {
                        parentFinished = false;
                        TryRunNode(parentNode);
                    }
                }
            }

            if(parentFinished && node.status == NodeStatus.None)
            {
                UnityEngine.Debug.Log("run node:" + node.name);
                var operate = node.Run();
                operate.RegistComplete(OnFinishNode);
                operate.RegistProgress(OnProgressNode);
            }
        }

        private void OnProgressNode(ScriptAINodeBase node)
        {
            
        }

        protected void OnFinishNode(ScriptAINodeBase node)
        {
            if(_subNodeMap.TryGetValue(node,out var subNodes))
            {
                foreach (var subNode in subNodes)
                {
                    TryRunNode(subNode);
                }
            }
        }

        public void Stop()
        {

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
            Stop();
        }
    }
}
