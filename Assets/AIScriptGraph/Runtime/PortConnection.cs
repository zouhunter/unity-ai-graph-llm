using UFrame.NodeGraph.DataModel;

using UnityEngine;

namespace AIScripting
{
    public class PortConnection : Connection
    {
        [InspectorName("���ȼ�")]
        public int priority;

        [InspectorName("����")]
        public bool disable;
    }
}