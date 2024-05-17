using System.Linq;
using System.Text;

using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Describe
{
    [CustomNode("TextAsset", 0, Define.GROUP)]
    public class TextAssetNode : DescribeBaseNode
    {
        public Ref<TextAsset> textfile;

        protected override AsyncOp WriteContent(StringBuilder sb)
        {
            if (!textfile.Value)
                return null;

            var fileName = textfile.Value.name;
            var fileInfo = textfile.Value.text;
            sb.AppendLine("文件名:");
            sb.AppendLine(fileName);
            sb.AppendLine("文件内容:");
            sb.AppendLine(fileInfo);
            return AsyncOp.CompletedOp;
        }
    }
}
