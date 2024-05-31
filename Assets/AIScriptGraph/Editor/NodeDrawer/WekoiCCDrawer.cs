using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;
using AIScripting.Work;

namespace AIScripting.Debugger
{
    [CustomEditor(typeof(OpenCCNode))]
    public class WekoiCCDrawer : Editor
    {
        private string apiResponse => node ? node.output.Value?.ToString() : null;
        private Vector2 ScrollPos;
        private OpenCCNode node;

        private void OnEnable()
        {
            node = target as OpenCCNode;
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

                string[] content = CodeResponceUtil.SplitContents(apiResponse);

                for (int i = 0; i < content.Length; i++)
                {
                    var str = content[i].Trim();
                    if (str == "")
                    {
                        continue;
                    }

                    CodeType codeType = CodeResponceUtil.CheckCodeType(str, out var scriptName, out var codeExt);

                    EditorGUILayout.BeginHorizontal();
                    var contentColor = GUI.contentColor;
                    if (codeType != CodeType.None)
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
                                    File.WriteAllText(scriptPath, CodeResponceUtil.GetContentScript(str));
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
                                    File.WriteAllText(scriptPath, CodeResponceUtil.GetContentScript(str));
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
    }
}
