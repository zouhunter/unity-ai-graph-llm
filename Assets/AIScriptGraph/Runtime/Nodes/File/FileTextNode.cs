using System.Linq;

using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting
{
    [CustomNode("FileText", 0, "AIScripting")]
    public class FileTextNode : ScriptNodeBase
    {
        [TextArea()]
        public string pofile_Text;
        public Ref<string> file_path;
        public Ref<string> out_put;

        protected override void OnProcess()
        {
            if(string.IsNullOrEmpty(file_path) || !System.IO.File.Exists(file_path))
            {
                DoFinish(false);
            }
            else
            {
                var fileName = System.IO.Path.GetFileName(file_path);
                var fileInfo = System.IO.File.ReadAllText(file_path);
                var textStr = new System.Text.StringBuilder();
                textStr.AppendLine(pofile_Text);
                textStr.AppendLine("�ļ���:");
                textStr.AppendLine(fileName);
                textStr.AppendLine("�ļ�����:");
                textStr.AppendLine(fileInfo);
                out_put.SetValue(textStr.ToString());
                DoFinish();
            }
        }
    }
}