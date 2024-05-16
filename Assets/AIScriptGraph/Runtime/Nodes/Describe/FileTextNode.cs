using System.Linq;
using System.Text;

using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Describe
{
    [CustomNode("FileText", 0, Define.GROUP)]
    public class FileTextNode : DescribeBaseNode
    {
        public Ref<string> file_path;

        protected override AsyncOp WriteContent(StringBuilder sb)
        {
            if (string.IsNullOrEmpty(file_path) || !System.IO.File.Exists(file_path))
                return null;

            var fileName = System.IO.Path.GetFileName(file_path);
            var fileInfo = System.IO.File.ReadAllText(file_path);
            sb.AppendLine("�ļ���:");
            sb.AppendLine(fileName);
            sb.AppendLine("�ļ�����:");
            sb.AppendLine(fileInfo);
            return AsyncOp.CompletedOp;
        }
    }
}