using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Export
{
    [CustomNode("FileWrite", group: Define.GROUP)]
    public class FileWriteNode : ScriptNodeBase
    {
        [Tooltip("name text")]
        public Ref<string> fileName;
        [Tooltip("info text")]
        public Ref<string> fileInfo;
        public bool append;
        public string ext = ".txt";
        public string exportDir = "Export";

        protected override void OnProcess()
        {
            if(!System.IO.Directory.Exists(exportDir))
            {
                System.IO.Directory.CreateDirectory(exportDir);
            }
            if(append)
            {
                System.IO.File.AppendAllText(exportDir + "/" + fileName.Value + ext, fileInfo.Value);
            }
            else
            {
                System.IO.File.WriteAllText(exportDir + "/" + fileName.Value + ext, fileInfo.Value);
            }
            DoFinish();
        }
    }
}