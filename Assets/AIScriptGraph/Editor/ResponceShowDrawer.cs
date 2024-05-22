using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace AIScripting.Debugger
{
    [CustomEditor(typeof(ResponceShowNode))]
    public class ResponceShowDrawer : Editor
    {
        private string apiResponse => node ? node.allText?.ToString() : null;
        private Vector2 ScrollPos;
        private ResponceShowNode node;

        public enum CodeType
        {
            None, CSharp, Shader, Other
        }

        private void OnEnable()
        {
            node = target as ResponceShowNode;
        }

        private string[] SplitContents(string text)
        {
            var contents = new List<string>();
            var lastIndex = 0;
            var startIndex = text.IndexOf("```");
            int inCodeBlock = 0;
            while (startIndex >= 0)
            {
                if (startIndex >= 0)
                {
                    inCodeBlock++;
                    if (startIndex > lastIndex)
                    {
                        if (inCodeBlock % 2 == 0)
                            startIndex += 3;

                        contents.Add(text.Substring(lastIndex, startIndex - lastIndex));
                        lastIndex = startIndex;

                        if (inCodeBlock % 2 != 0)
                            startIndex += 3;
                    }
                    else
                    {
                        startIndex += 3;
                    }
                    startIndex = text.IndexOf("```", startIndex);
                }
                else
                {
                    break;
                }
            }
            if (text.Length > lastIndex)
            {
                contents.Add(text.Substring(lastIndex));
            }
            return contents.ToArray();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!string.IsNullOrEmpty(apiResponse))
            {
                GUILayout.Label("API Response:");
                bool val = EditorStyles.textField.wordWrap;
                EditorStyles.textField.wordWrap = true;

                ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

                string[] content = SplitContents(apiResponse);

                for (int i = 0; i < content.Length; i++)
                {
                    var str = content[i].Trim();
                    if (str == "")
                    {
                        continue;
                    }

                    CodeType codeType = CheckCodeType(str,out var scriptName,out var codeExt);

                    EditorGUILayout.BeginHorizontal();
                    var contentColor = GUI.contentColor;
                    if(codeType != CodeType.None)
                        GUI.contentColor = Color.cyan;
                    EditorGUILayout.TextArea(str, GUILayout.ExpandWidth(true));
                    GUI.contentColor = contentColor;

                    EditorGUILayout.BeginVertical();
                    if (GUILayout.Button("Copy", GUILayout.Width(50)))
                    {
                        TextEditor te = new TextEditor();
                        te.text = content[i].Trim();
                        te.SelectAll();
                        te.Copy();
                    }

                    if (codeType != CodeType.None && !string.IsNullOrEmpty(scriptName))
                    {
                        if (!string.IsNullOrEmpty(node.saveFilePath) && GUILayout.Button("Create", GUILayout.Width(50)))
                        {
                            if (AssetDatabase.FindAssets(scriptName).Length > 0)
                            {
                                if (EditorUtility.DisplayDialog("Script already exists", "A script with the name " + scriptName + " already exists. Do you want to overwrite it? (Note: Be careful!)", "Yes", "No"))
                                {
                                    string scriptPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(scriptName)[0]);
                                    File.WriteAllText(scriptPath, GetContentScript(str));
                                    AssetDatabase.Refresh();
                                    EditorUtility.DisplayDialog("Script overwritten", "The script " + scriptName + " was overwritten.", "Ok");

                                    EditorApplication.delayCall += () =>
                                    {
                                        Selection.activeObject = AssetDatabase.LoadAssetAtPath(scriptPath, typeof(MonoScript));
                                    };
                                }
                            }
                            else
                            {
                                string scriptPath = node.saveFilePath + "/" + scriptName + "." + codeExt;
                                if (scriptPath.Length != 0)
                                {
                                    File.WriteAllText(scriptPath, GetContentScript(str));
                                    AssetDatabase.Refresh();

                                    EditorApplication.delayCall += () =>
                                    {
                                        Selection.activeObject = AssetDatabase.LoadAssetAtPath(scriptPath, typeof(MonoScript));
                                    };
                                }
                            }

                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 获取csharp脚本名称
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string GetCSharpScriptName(string content)
        {
            var match = Regex.Match(content, "public class (\\w+)");
            if(match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        /// <summary>
        /// 获取shader脚本名称    
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string GetShaderScriptName(string content)
        {
            var match = Regex.Match(content, "Shader \"(.*)\"");
            if (match.Success)
            {
                var fullname = match.Groups[1].Value;
                var index = fullname.LastIndexOf('/');
                if(index > 0)
                {
                    return fullname.Substring(index + 1);
                }
                return fullname;
            }
            return null;
        }

        /// <summary>
        /// 获取代码内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string GetContentScript(string content)
        {
            return Regex.Replace(content, @"```(\w*)", ""); 
        }

        /// <summary>
        /// 分析代码类型
        /// </summary>
        /// <param name="content"></param>
        /// <param name="scriptName"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        private CodeType CheckCodeType(string content,out string scriptName,out string fileExt)
        {
            fileExt = null;
            scriptName = null;
            var match = Regex.Match(content, @"```(\w+)");
            if (match.Success)
            {
                var codeName = match.Groups[1].Value.ToLower();
                switch (codeName)
                {
                    case "csharp":
                    case "c#":
                        fileExt = "cs";
                        scriptName = GetCSharpScriptName(content);
                        return CodeType.CSharp;
                    case "shader":
                    case "glsl":
                    case "hlsl":
                        fileExt = "shader";
                        scriptName = GetShaderScriptName(content);
                        return CodeType.Shader;
                    default:
                        break;
                }
            }
            return CodeType.None;
        }
    }
}
