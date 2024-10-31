using MateAI.ScriptableBehaviourTree.Actions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using UnityEditor;

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    [System.Serializable]
    public class BTree : ScriptableObject, IVariableProvider
    {
        [SerializeField]
        protected TreeInfo _rootTree;
        public virtual TreeInfo rootTree
        {
            get { return _rootTree; }
            set { _rootTree = value; }
        }
        public bool TreeStarted => _treeStarted;
        public bool LogInfo;
        private Dictionary<Type, IEnumerable<FieldInfo>> _fieldMap = new Dictionary<Type, IEnumerable<FieldInfo>>();
        private bool _treeStarted;
        private BTree _ownerTree;
        public BTree Owner
        {
            get
            {
                return _ownerTree ?? this;
            }
        }
        public int TickCount { get; private set; }

        #region Variables
        protected VariableCenter _variableCenter = new VariableCenter();

        public VariableCenter Variables => _variableCenter;

        public void BindingExtraVariable(Func<string, Variable> variableGetter)
        {
            _variableCenter.BindingExtraVariable(variableGetter);
        }

        public Variable GetVariable(string name)
        {
            return _variableCenter.GetVariable(name);
        }

        public Variable<T> GetVariable<T>(string name)
        {
            return _variableCenter.GetVariable<T>(name);
        }

        public Variable<T> GetVariable<T>(string name, bool createIfNotExits)
        {
            return _variableCenter.GetVariable<T>(name, createIfNotExits);
        }

        public T GetVariableValue<T>(string name)
        {
            return _variableCenter.GetVariableValue<T>(name);
        }

        public bool TryGetVariable<T>(string name, out Variable<T> variable)
        {
            return _variableCenter.TryGetVariable(name, out variable);
        }

        public bool TryGetVariable(string name, out Variable variable)
        {
            return _variableCenter.TryGetVariable(name, out variable);
        }

        public void SetVariable(string name, Variable variable)
        {
            _variableCenter.SetVariable(name, variable);
        }
        public bool SetVariableValue(string name, object data)
        {
            return _variableCenter.SetVariableValue(name, data);
        }
        public void SetVariableValue<T>(string name, T data)
        {
            _variableCenter.SetVariableValue(name, data);
        }
        #endregion Variables

        #region Events
        private HashSet<string> _persistentEvents = new HashSet<string>();
        private Dictionary<string, List<Action<object>>> _events = new Dictionary<string, List<Action<object>>>();
        public void BindingEventMap(Dictionary<string, List<Action<object>>> map)
        {
            this._events = map;
        }
        public void RegistEvent(string eventKey, Action<object> callback)
        {
            if (!_events.TryGetValue(eventKey, out var actions))
            {
                _events[eventKey] = new List<Action<object>>() { callback };
            }
            else
            {
                actions.Add(callback);
            }
        }
        public void RemoveEvent(string eventKey, Action<object> callback)
        {
            if (_events.TryGetValue(eventKey, out var actions))
            {
                actions.Remove(callback);
            }
        }
        public void SendEvent(string eventKey, object arg = null)
        {
            if (_events.TryGetValue(eventKey, out var actions))
            {
                for (int i = actions.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        actions[i]?.Invoke(arg);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
        #endregion

        #region TreeTraversal
        public BTree CreateInstance()
        {
            return Instantiate(this);
        }

        public void CollectToDic(TreeInfo info, Dictionary<string, TreeInfo> infoDic)
        {
            if (info == null || info.id == null)
                return;

            infoDic[info.id] = info;
            info.subTrees?.ForEach(t =>
            {
                CollectToDic(t, infoDic);
            });
        }

        public virtual TreeInfo FindTreeInfo(string id, bool deep = true)
        {
            var dic = new Dictionary<string, TreeInfo>();
            CollectToDic(rootTree, dic);
            if (dic.TryGetValue(id, out TreeInfo treeInfo))
            {
                return treeInfo;
            }
            return null;
        }

        /// <summary>
        /// 清理上下文
        /// </summary>
        /// <param name="includePersistent"></param>
        public void ClearCondition(bool includePersistent = true)
        {
            _variableCenter.ClearCondition(includePersistent);
            if (includePersistent)
            {
                _events.Clear();
            }
            else
            {
                var keys = new List<string>(_events.Keys);
                foreach (var key in keys)
                {
                    if (!_persistentEvents.Contains(key))
                    {
                        _events.Remove(key);
                    }
                }
            }
        }

        public virtual bool StartUp()
        {
            if (rootTree != null && rootTree.node != null)
            {
                TickCount = 0;
                SetOwnerDeepth(rootTree, this);
                _treeStarted = true;
                return true;
            }
            Debug.LogError("rootTree empty!" + (rootTree == null));
            return false;
        }

        public void Stop()
        {
            _treeStarted = false;
            CleanDeepth(rootTree);
        }


        public bool ResetStart()
        {
            if (rootTree != null && _treeStarted)
            {
                TickCount = 0;
                SetOwnerDeepth(rootTree, this);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 深度绑定
        /// </summary>
        /// <param name="info"></param>
        /// <param name="owner"></param>
        public void SetOwnerDeepth(TreeInfo info, BTree owner)
        {
            _ownerTree = owner;
            LogInfo = owner.LogInfo;
            info.node?.SetOwner(owner);
            info.status = Status.Inactive;
            info.tickCount = 0;
            if (info.condition.enable && info.condition.conditions != null && info.node == this)
            {
                foreach (var condition in info.condition.conditions)
                {
                    condition.status = Status.Inactive;
                    condition.tickCount = 0;
                    condition.node?.SetOwner(owner);
                    condition.subConditions?.ForEach(subNode =>
                    {
                        subNode.status = Status.Inactive;
                        subNode.tickCount = 0;
                        subNode?.node?.SetOwner(owner);
                    });
                }
            }
            if (info.subTrees != null && info.subTrees != null)
            {
                foreach (var subInfo in info.subTrees)
                {
                    if (subInfo.enable)
                    {
                        SetOwnerDeepth(subInfo, owner);
                    }
                }
            }
        }

        /// <summary>
        /// 深度清理
        /// </summary>
        /// <param name="info"></param>
        public void CleanDeepth(TreeInfo info)
        {
            if (info.subTrees != null && info.subTrees != null)
            {
                foreach (var subInfo in info.subTrees)
                {
                    if (subInfo.enable)
                    {
                        CleanDeepth(subInfo);
                    }
                }
            }
            if (info.condition.enable && info.condition.conditions != null && info.node == this)
            {
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.subConditions != null)
                    {
                        for (int i = 0; i < condition.subConditions.Count; i++)
                        {
                            var condition2 = condition.subConditions[i];
                            condition2.node?.Clean();
                        }
                    }
                    condition.node?.Clean();
                }
            }
            info.node?.Clean();
        }
        /// <summary>
        /// 刷新
        /// </summary>
        public virtual Status Tick()
        {
            if (rootTree == null || rootTree.node == null || rootTree.enable == false)
            {
                if (rootTree == null)
                    Debug.LogError("BTree rootTree == null" + name);
                if (rootTree.node == null)
                    Debug.LogError("BTree rootTree.node == null" + name);
                if (!rootTree.enable)
                    Debug.LogError("BTree rootTree.enable == false" + name);
                return Status.Inactive;
            }
            TickCount++;
            rootTree.status = OnUpdate();
            return rootTree.status;
        }

        internal void OnReset()
        {
            if (rootTree != null && rootTree.node != null)
            {
                SetOwnerDeepth(rootTree, Owner ?? this);
            }
        }

        internal Status OnUpdate()
        {
            return rootTree.node.Execute(rootTree);
        }

        /// <summary>
        /// 嵌套节点检查
        /// </summary>
        /// <param name="matchType"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public virtual bool CheckConditions(TreeInfo treeInfo, MatchType matchType, List<SubConditionItem> conditions)
        {
            if (conditions != null && conditions.Count > 0)
            {
                int matchCount = 0;
                int validCount = 0;

                foreach (var conditionNode in conditions)
                {
                    if (conditionNode != null && conditionNode.node && conditionNode.state < 2)
                        validCount++;
                    else
                        continue;

                    conditionNode.status = conditionNode.node.Execute(treeInfo);
                    conditionNode.tickCount = Owner.TickCount;

                    var result = conditionNode.status == Status.Success;
                    if (conditionNode.state == 1)
                        result = !result;
#if UNITY_EDITOR
                    if (LogInfo)
                    {
                        Debug.Log("check sub condition:" + conditionNode.node.name + " ," + result);
                    }
#endif
                    switch (matchType)
                    {
                        case MatchType.AnySuccess:
                            if (result)
                                return true;
                            break;
                        case MatchType.AnyFailure:
                            if (!result)
                                return true;
                            break;
                        case MatchType.AllSuccess:
                            if (result)
                                matchCount++;
                            else
                                return false;
                            break;
                        case MatchType.AllFailure:
                            if (!result)
                                matchCount++;
                            else
                                return false;
                            break;
                        default:
                            matchCount = -1;
                            break;
                    }
                }
                if (matchCount >= 0 && matchCount != validCount)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 条件检查
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual bool CheckConditions(TreeInfo treeInfo)
        {
            var conditionInfo = treeInfo.condition;
            if (conditionInfo.enable && conditionInfo.conditions.Count > 0)
            {
                int matchCount = 0;
                int validCount = 0;

                foreach (var condition in conditionInfo.conditions)
                {
                    if (condition != null && condition.node != null && condition.state < 2)
                        validCount++;
                    else
                        continue;

                    bool subResult = true;
                    if (condition.subEnable)
                        subResult = CheckConditions(treeInfo, condition.matchType, condition.subConditions);

                    if (subResult)
                        condition.status = condition.node.Execute(treeInfo);
                    else
                        condition.status = Status.Failure;

                    condition.tickCount = Owner.TickCount;
                    var checkResult = condition.status == (condition.state == 1 ? Status.Failure : Status.Success);
#if UNITY_EDITOR
                    if (LogInfo)
                        Debug.Log("checking condition:" + condition.node.name + "," + checkResult + "," + conditionInfo.matchType);
#endif
                    switch (conditionInfo.matchType)
                    {
                        case MatchType.AllSuccess:
                            if (checkResult)
                                matchCount++;
                            else
                                return false;
                            break;
                        case MatchType.AllFailure:
                            if (!checkResult)
                                matchCount++;
                            else
                                return false;
                            break;
                        case MatchType.AnySuccess:
                            if (checkResult)
                                return true;
                            matchCount = -1;
                            break;
                        case MatchType.AnyFailure:
                            if (!checkResult)
                                return true;
                            matchCount = -1;
                            break;
                    }
                }
                return matchCount >= 0 && matchCount == validCount;
            }
            return true;
        }
        #endregion


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
        /// <summary>
        /// 持久变量
        /// </summary>
        /// <param name="variableName"></param>
        public void SetPersistentVariable(string variableName)
        {
            _variableCenter.SetPersistentVariable(variableName);
        }
        /// <summary>
        /// 持久事件
        /// </summary>
        /// <param name="eventName"></param>
        public void SetPersistentEvent(string eventName)
        {
            _persistentEvents.Add(eventName);
        }

        /// <summary>
        /// 收集节点
        /// </summary>
        /// <param name="allNodes"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void CollectNodesDeepth(TreeInfo info, List<BaseNode> nodes)
        {
            if (info.node && !nodes.Contains(info.node))
            {
                nodes.Add(info.node);
            }
            if (info.condition != null && info.condition.conditions != null)
            {
                int i = 0;
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.node && !nodes.Contains(condition.node))
                    {
                        nodes.Add(condition.node);
                    }

                    if (condition.subConditions != null)
                    {
                        int j = 0;
                        foreach (var subNode in condition.subConditions)
                        {
                            if (subNode != null && subNode.node && !nodes.Contains(subNode.node))
                            {
                                nodes.Add(subNode.node);
                            }
                            j++;
                        }
                    }
                    i++;
                }
            }
            if (info.subTrees != null)
            {
                for (int i = 0; i < info.subTrees.Count; i++)
                {
                    var item = info.subTrees[i];
                    CollectNodesDeepth(item, nodes);
                }
            }
        }
    }
}
