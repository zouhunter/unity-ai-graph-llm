
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
    }
}