using UFrame.NodeGraph;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace AIScripting.Describe
{
    [CustomNode("DirFilesText", 1, Define.GROUP)]
    public class DirFilesTextNode : DescribePrefixNode
    {
        public string fileExt = "*.cs";
        [Tooltip("文件夹")]
        public Ref<string> dir_path;
        [Tooltip("遍历")]
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
