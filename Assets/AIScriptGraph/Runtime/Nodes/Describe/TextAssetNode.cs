using System.Linq;
using System.Text;

using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Describe
{
    [CustomNode("TextAsset", 1, Define.GROUP)]
    public class TextAssetNode : DescribePrefixNode
    {
        public Ref<TextAsset> textfile;
        public bool codeOnly;
        protected override AsyncOp WriteContent(StringBuilder sb)
        {
            if (!textfile.Value)
                return null;
            var fileInfo = textfile.Value.text;
            if (!codeOnly)
            {
                var fileName = textfile.Value.name;
                sb.AppendLine("文件名:");
                sb.AppendLine(fileName);
                sb.AppendLine("文件内容:");
            }
            sb.AppendLine(fileInfo);
            return AsyncOp.CompletedOp;
        }
    }
}
