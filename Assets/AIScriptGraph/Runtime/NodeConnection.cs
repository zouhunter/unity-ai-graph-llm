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

        [Tooltip("���ȼ�")]
        public int priority;

        [Tooltip("����")]
        public bool disable;

        [Tooltip("����")]
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