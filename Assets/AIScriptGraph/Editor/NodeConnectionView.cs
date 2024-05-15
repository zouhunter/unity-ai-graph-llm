
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;
using UFrame.NodeGraph.DefultSkin;
using UnityEditor;
using UnityEngine;

namespace AIScripting
{
    [CustomView(typeof(NodeConnection))]
    public class NodeConnectionView : ConnectionView
    {
        public override Color LineColor
        {
            get
            {
                if (target is NodeConnection portConnection)
                {
                    if (portConnection.disable)
                    {
                        return Color.red;
                    }
                }
                return base.LineColor;
            }
        }

        public override void OnDrawLabel(Vector2 centerPos, string label)
        {
            //base.OnDrawLabel(centerPos, label);
        }
    }
}