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

        [Tooltip("���ȼ�")]
        public int priority;

        [Tooltip("����")]
        public bool disable;

        [Tooltip("����")]
        public List<Condition> conditions;

        [Tooltip("��������")]
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