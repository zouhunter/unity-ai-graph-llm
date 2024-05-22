using System.Linq;
using System.Text;

using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Describe
{
    [CustomNode("FileText", 1, Define.GROUP)]
    public class FileTextNode : DescribePrefixNode
    {
        public Ref<string> file_path;

        protected override AsyncOp WriteContent(StringBuilder sb)
        {
            if (string.IsNullOrEmpty(file_path) || !System.IO.File.Exists(file_path))
                return null;

            var fileName = System.IO.Path.GetFileName(file_path);
            var fileInfo = System.IO.File.ReadAllText(file_path);
            sb.AppendLine("文件名:");
            sb.AppendLine(fileName);
            sb.AppendLine("文件内容:");
            sb.AppendLine(fileInfo);
            return AsyncOp.CompletedOp;
        }
    }
}
