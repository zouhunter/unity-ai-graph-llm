/*-*-* Copyright (c) Work@uframe
 * Author: 
 * Creation Date: 2024-05-21
 * Version: 1.0.0
 * Description: 
 *_*/

using UnityEngine;
using UFrame.NodeGraph;
using System.Runtime.InteropServices;

namespace AIScripting.Debugger
{
    [CustomNode("FileLocat", group: Define.GROUP)]
    public class FileLocatNode : ScriptNodeBase
    {
        [Tooltip("文件路径")]
        public Ref<string> filePath;

        protected override void OnProcess()
        {
            if (string.IsNullOrEmpty(this.filePath.Value))
            {
                Debug.LogError("文件路径为空");
                base.DoFinish(false);
                return;
            }
            var filePath = System.IO.Path.GetFullPath(this.filePath.Value).Replace("\\", "/");
            if(!System.IO.File.Exists(filePath))
            {
                Debug.LogError("文件不存在");
                base.DoFinish(false);
                return;
            }
#if UNITY_EDITOR
            if (filePath.StartsWith(Application.dataPath))
            {
                filePath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, filePath);
                UnityEditor.EditorUtility.RevealInFinder(filePath);
            }
            else
#endif
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN ||UNITY_STANDALONE_OSX
                OpenFolderAndSelectFile(filePath);
#endif
            }
            DoFinish(true);
        }
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(System.IntPtr hWnd);

    public static void OpenFolderAndSelectFile(string filePath)
    {
        string argument = $"/select, \"{filePath}\"";
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "explorer.exe";
        process.StartInfo.Arguments = argument;
        process.Start();
        
        process.WaitForInputIdle();
        SetForegroundWindow(process.MainWindowHandle);
    }

#elif UNITY_STANDALONE_OSX
    public static void OpenFolderAndSelectFile(string filePath)
    {
        string directoryPath = System.IO.Path.GetDirectoryName(filePath);
        string fileName = System.IO.Path.GetFileName(filePath);
        string argument = $"-R \"{fileName}\"";
        
        Process process = new Process();
        process.StartInfo.FileName = "open";
        process.StartInfo.Arguments = argument;
        process.StartInfo.WorkingDirectory = directoryPath;
        process.Start();
    }
#endif
    }
}

