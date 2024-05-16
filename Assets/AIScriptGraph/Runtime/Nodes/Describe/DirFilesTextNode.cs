using UFrame.NodeGraph;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace AIScripting.Describe
{
    [CustomNode("DirFilesText", 0, Define.GROUP)]
    public class DirFilesTextNode : DescribeBaseNode
    {
        public string fileExt = "*.cs";
        [Tooltip("�ļ���")]
        public Ref<string> dir_path;
        [Tooltip("����")]
        public bool recursive;

        protected override AsyncOp WriteContent(StringBuilder sb)
        {
            if (!string.IsNullOrEmpty(dir_path) && System.IO.Directory.Exists(dir_path))
            {
                var files = System.IO.Directory.GetFiles(dir_path, fileExt, recursive ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    sb.AppendLine( System.IO.File.ReadAllText(file));
                }
                if(files.Length > 0)
                    return AsyncOp.CompletedOp;

                DoFinish(files.Length > 0);
            }
            return null;
        }
    }
}