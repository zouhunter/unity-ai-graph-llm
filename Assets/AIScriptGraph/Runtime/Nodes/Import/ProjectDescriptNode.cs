using LitJson;

using System.Collections;
using System.Collections.Generic;
using System.Text;

using UFrame.NodeGraph;

using UnityEngine;

namespace AIScripting.Import
{
    [CustomNode("ProjectDescript", group: Define.GROUP)]
    public class ProjectDescriptNode : ScriptNodeBase
    {
        [Tooltip("描述文本")]
        public Ref<TextAsset> descTextAsset;
        [Tooltip("输出文本")]
        public Ref<string> outputText;
        public List<string> fileTracks;

        protected override void OnProcess()
        {
            if (descTextAsset.Value != null)
            {
                var text = DecodeText(descTextAsset.Value.text);
                if (!string.IsNullOrEmpty(text))
                {
                    Debug.Log("ProjectDescript:" + text);
                    outputText.SetValue(text);
                    DoFinish(true);
                    return;
                }
                Debug.LogWarning("ProjectDescriptNode:faild DecodeText text!");
            }
            Debug.LogWarning("ProjectDescriptNode:faild get desc text!");
            DoFinish(false);
        }

        private void CreateDirTree(string folder,StringBuilder sb)
        {
            var dir = new System.IO.DirectoryInfo(folder);
            var refFolder = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, folder);
            sb.AppendLine($"{refFolder}:[".Replace('\\','/'));
            var index = 0;
            foreach (var item in dir.GetFiles())
            {
                if (!fileTracks.Contains(item.Extension.ToLower()))
                    continue;
                if(index++ > 0)
                    sb.Append(",");
                sb.Append(item.Name);
            }
            sb.AppendLine("]");
            foreach (var item in dir.GetDirectories())
            {
                CreateDirTree(folder + "/" + item.Name, sb);
            }
        }

        public string DecodeText(string text)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# directory descs:");
            var lines = text.Split('\n');
            int index = 0;
            List<string> subFolders = new List<string>();
            foreach (var line in lines)
            {
                var textLine = line.Trim();
                if (string.IsNullOrEmpty(textLine))
                    continue;

                var pair = textLine.Split(':');
                if (pair.Length == 2)
                {
                    var dir = pair[0];
                    var desc = pair[1];
                    
                    if(dir.EndsWith('/'))
                    {
                        dir = dir.Substring(0,dir.Length - 1);
                        subFolders.Add(dir);
                    }
                    sb.AppendLine($"{++index}.{desc}:{dir}");
                }
            }
            sb.AppendLine("# files of directorys:");
            foreach (var dir in subFolders)
            {
                var dirFullPath = System.IO.Path.GetFullPath(dir);
                if (System.IO.Directory.Exists(dirFullPath))
                {
                    var files = System.IO.Directory.GetFiles(dirFullPath, "*.*", System.IO.SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        CreateDirTree(dirFullPath, sb);
                    }
                }
            }

            return sb.ToString();
        }
    }
}

