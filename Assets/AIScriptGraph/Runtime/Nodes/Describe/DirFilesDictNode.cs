using UFrame.NodeGraph;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace AIScripting.Describe
{
    [CustomNode("DirFilesDict", 1, Define.GROUP)]
    public class DirFilesDictNode : ScriptNodeBase
    {
        public string fileExt = "*.cs";
        [Tooltip("文件夹")]
        public Ref<string> dir_path;
        [Tooltip("遍历")]
        public bool recursive;
        public Ref<Dictionary<string, string>> fileInfoDict;

        protected override void OnProcess()
        {
            fileInfoDict.SetValue(new Dictionary<string, string>());
            var files = System.IO.Directory.GetFiles(dir_path, fileExt, recursive ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var fileName = System.IO.Path.GetFileName(file);
                fileInfoDict.Value[fileName] = System.IO.File.ReadAllText(file);
                Debug.Log(fileInfoDict.Value[fileName]);
            }
            DoFinish();
        }
    }
}
