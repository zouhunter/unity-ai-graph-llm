using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Import
{
    [CustomNode("FileRead", 1, Define.GROUP)]
    public class FileReadNode : ScriptNodeBase
    {
        [Tooltip("�ļ�·��")]
        public Ref<string> file_path;
        [Tooltip("�����ļ���")]
        public Ref<string> exportfileName;
        [Tooltip("�����ļ���Ϣ")]
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
