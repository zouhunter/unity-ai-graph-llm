using UFrame.NodeGraph;
using System.Collections.Generic;
using UnityEngine;

namespace AIScripting
{
    [CustomNode("DirFilesText", 0, "AIScripting")]
    public class DirFilesTextNode : ScriptNodeBase
    {
        [TextArea]
        public string prefileText = "...�����һ��:";
        public string fileExt = "*.cs";
        [Tooltip("�ļ���")]
        public Ref<string> dir_path;
        [Tooltip("����")]
        public bool recursive;
        [Tooltip("�����")]
        public Ref<string[]> out_put;

        protected override void OnProcess()
        {
            if (string.IsNullOrEmpty(dir_path) || !System.IO.Directory.Exists(dir_path))
            {
                DoFinish(false);
            }
            else
            {
                var outs = new List<string>();
                var files = System.IO.Directory.GetFiles(dir_path, fileExt, recursive ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    outs.Add(prefileText + System.IO.File.ReadAllText(file));
                }
                out_put.SetValue(outs.ToArray());
                DoFinish(files.Length > 0);
            }
        }
    }
}