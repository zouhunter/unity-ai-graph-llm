using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

namespace AIScripting
{
    public class PortConnection : Connection
    {
        public PortConnection(string type)
        {
            this.type = type;
        }
    }
}