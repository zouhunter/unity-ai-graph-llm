
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
    public class ScriptingNodeView : BaseSkinNodeView
    {
        public override float SuperHeight
        {
            get
            {
                return -EditorGUIUtility.singleLineHeight * 0.5f;
            }
        }
        public override string Category => "ai";
        public override int Style
        {
            get
            {
                if(target is ScriptNodeBase node)
                {
                    return node.Style;
                }
                return base.Style;
            }
        }

        public override void OnNodeGUI(Rect position, NodeData data)
        {
            base.OnNodeGUI(position, data);
            if(target is ScriptNodeBase scriptNode)
            {
                if(scriptNode.status == Status.Running)
                {
                    var progressRect = new Rect(position.x,position.y,position.width,5);
                    EditorGUI.ProgressBar(progressRect, scriptNode.progress, "Running");
                }
            }
        }
    }
}