/*-*-* Copyright (c) mateai@wekoi
 * Author: 
 * Creation Date: 2024-05-21
 * Version: 1.0.0
 * Description: 
 *_*/

using UnityEngine;
using UFrame.NodeGraph;
using System.Text;

namespace AIScripting.Debugger
{
    [CustomNode("DiffShow", group: Define.GROUP)]
    public class DiffShowNode : ScriptNodeBase
    {
        [Tooltip("源文本")]
        public Ref<string> sourceText;
        [Tooltip("目标文本")]
        public Ref<string> targetText;
        public bool scriptOnly = true;

        public override void ResetGraph(AIScriptGraph graph)
        {
            base.ResetGraph(graph);
        }
        protected override void OnProcess()
        {
            DoFinish(true);
        }
    }
}

