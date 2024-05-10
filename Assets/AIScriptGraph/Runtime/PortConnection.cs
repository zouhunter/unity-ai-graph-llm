using UFrame.NodeGraph.DataModel;

using UnityEngine;

namespace AIScripting
{
    public class PortConnection : Connection
    {
        [InspectorName("优先级")]
        public int priority;

        [InspectorName("禁用")]
        public bool disable;
    }
}