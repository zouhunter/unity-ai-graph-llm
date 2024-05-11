using UFrame.NodeGraph;

using UnityEngine;

namespace AIScripting
{
    [CustomNode("DirFilesText", 0, "AIScripting")]
    public class DirFilesTextNode : ScriptNodeBase
    {
        public string beforeText = "# ������һ���ļ�:";
        public string splitText = "## ��{0}���ļ�:";
        public string fileExt = "*.cs";
        [Tooltip("�ļ���")]
        public Ref<string> dir_path;
        [Tooltip("����")]
        public bool recursive;
        [Tooltip("�����")]
        public Ref<string> out_put;

        protected override void OnProcess()
        {
            if (string.IsNullOrEmpty(dir_path) || !System.IO.Directory.Exists(dir_path))
            {
                DoFinish(false);
            }
            else
            {
                var fileInfo = new System.Text.StringBuilder();
                fileInfo.AppendLine(beforeText);
                var files = System.IO.Directory.GetFiles(dir_path, fileExt, recursive ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    fileInfo.AppendLine(string.Format(splitText, i));
                    fileInfo.AppendLine(System.IO.File.ReadAllText(file));
                }
                out_put.SetValue(fileInfo.ToString());
                DoFinish(files.Length > 0);
            }
        }
    }
}