using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Import
{
    [CustomNode("FileRead", 1, Define.GROUP)]
    public class FileReadNode : ScriptNodeBase
    {
        [Tooltip("文件路径")]
        public Ref<string> file_path;
        [Tooltip("导出文件名")]
        public Ref<string> exportfileName;
        [Tooltip("导出文件信息")]
        public Ref<string> exportfileInfo;

        protected override void OnProcess()
        {
            if (string.IsNullOrEmpty(file_path) || !System.IO.File.Exists(file_path))
            {
                DoFinish(false);
                return;
            }
            exportfileName.SetValue(System.IO.Path.GetFileName(file_path));
            exportfileInfo.SetValue(System.IO.File.ReadAllText(file_path));
            DoFinish(true);
        }
    }
}
