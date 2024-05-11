using UFrame.NodeGraph.DataModel;

using UnityEngine;

namespace AIScripting
{
    public class PortConnection : Connection
    {
        [SerializeField]
        private string _title;
        public override string Title => string.IsNullOrEmpty(_title)? base.Title:_title;

        [Tooltip("���ȼ�")]
        public int priority;

        [Tooltip("����")]
        public bool disable;
    }
}