using System.Collections.Generic;

using UFrame.NodeGraph.DataModel;

using UnityEngine;

namespace AIScripting
{
    public class NodeConnection : Connection
    {
        [SerializeField]
        protected string _title;

        public override string Title => string.IsNullOrEmpty(_title) ? base.Title : _title;

        [Tooltip("优先级")]
        public int priority;

        [Tooltip("禁用")]
        public bool disable;

        [Tooltip("条件")]
        public List<Condition> conditions;

        [Tooltip("条件类型")]
        public ConditionType conditionType;
        public enum ConditionType
        {
            And,
            Or,
        }

        public bool Pass(IVariableProvider provider)
        {
            if (disable)
                return false;
            if (conditions == null || conditions.Count == 0)
                return true;
            switch (conditionType)
            {
                case ConditionType.And:
                    return conditions.TrueForAll(c => c.Check(provider));
                case ConditionType.Or:
                    return conditions.Exists(c => c.Check(provider));
            }
            return false;
        }
    }
}
