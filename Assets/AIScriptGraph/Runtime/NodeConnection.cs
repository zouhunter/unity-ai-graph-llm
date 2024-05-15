using System.Collections.Generic;
using UFrame.NodeGraph.DataModel;
using UnityEngine;

namespace AIScripting
{
    public class NodeConnection : Connection
    {
        [SerializeField]
        protected string _title;

        public override string Title => string.IsNullOrEmpty(_title)? base.Title:_title;

        [Tooltip("优先级")]
        public int priority;

        [Tooltip("禁用")]
        public bool disable;

        [Tooltip("条件")]
        public List<Condition> conditions;

        public bool Pass()
        {
            if (disable)
                return false;
            //TODO check conditions
            return false;
        }
    }
}