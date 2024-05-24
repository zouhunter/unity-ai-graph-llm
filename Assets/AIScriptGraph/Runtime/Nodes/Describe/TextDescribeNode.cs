using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEngine;

namespace AIScripting.Describe
{
    [CustomNode("Text",orderPriority:1, group: Define.GROUP)]
    public class TextDescribeNode : DescribeTextAttachNode
    {
        [Tooltip("文本说明")]
        public Ref<string> input_text;

        protected override AsyncOp WriteContent(StringBuilder sb)
        {
            if (!string.IsNullOrEmpty(input_text.Value))
            {
                sb.AppendLine(input_text.Value);
            }
            return AsyncOp.CompletedOp;
        }
    }
}
