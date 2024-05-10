using UFrame.NodeGraph;

namespace AIScripting
{
    [CustomNode("FileText", 0, "AIScripting")]
    public class FileTextNode : ScriptNodeBase
    {
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
                var fileInfo = System.IO.File.ReadAllText(file_path);
                out_put.Value = fileInfo;
                DoFinish();
            }
        }
    }
}