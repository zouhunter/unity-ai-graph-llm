using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Text;
using System.ComponentModel;
using static UnityEditor.Progress;

namespace AIScripting.Debugger
{
    [CustomEditor(typeof(DiffShowNode))]
    public class DiffShowDrawer : Editor
    {
        private DiffShowNode node;
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private GUIStyle _textStyle;
        private List<int> _leftDetail;
        private List<int> _rightDetail;
        private KeyValuePair<List<int>, List<int>> _diffs;
        private string _lastTarget;
        private string _lastSource;

        private void OnEnable()
        {
            node = target as DiffShowNode;
        }

        private void CalcuteDiff(string source, string target)
        {
            if (_lastSource == source && _lastTarget == target && _leftDetail != null && _rightDetail != null)
                return;
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return;
            _lastSource = source;
            _lastTarget = target;

            source = "source\n" + source;
            target = "target\n" + target;
            string[] src_array = source.Split('\n');
            for (int i = 0; i < src_array.Length; i++)
                src_array[i] = src_array[i].Trim();

            string[] des_array = target.Split('\n');
            for (int i = 0; i < des_array.Length; i++)
                des_array[i] = des_array[i].Trim();

            var trace = MyersUtil.Myers_FindTrace(src_array, des_array);
            var stateIDs = MyersUtil.Myers_FindStates(src_array.Length, des_array.Length, trace);
            stateIDs.RemoveAt(0);
            stateIDs.RemoveAt(0);
            Debug.Log("stateId:" + string.Join(',',stateIDs));
            var way = MyersUtil.Mayers_StateDetail(stateIDs);
            _leftDetail = way.Key;
            _rightDetail = way.Value;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!node)
                return;
            if (_textStyle == null)
            {
                _textStyle = new GUIStyle(EditorStyles.textArea);
                _textStyle.richText = true;
            }

            var width = EditorGUIUtility.currentViewWidth - 6;
            bool val = EditorStyles.textField.wordWrap;
            EditorStyles.textField.wordWrap = true;
            var targetScript = node.allText?.ToString();
            if (!string.IsNullOrEmpty(targetScript) && node.scriptOnly)
            {
                var contents = CodeResponceUtil.SplitContents(targetScript);
                if (contents != null)
                {
                    foreach (var content in contents)
                    {
                        var codeType = CodeResponceUtil.CheckCodeType(content, out var scriptName, out var fileExt);
                        if (codeType != CodeType.None)
                        {
                            targetScript = CodeResponceUtil.GetContentScript(content);
                        }
                    }
                }
            }

            CalcuteDiff(node.sourceText.Value, targetScript);

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                using (var ver = new EditorGUILayout.VerticalScope(GUILayout.Width(width * 0.5f), GUILayout.ExpandHeight(true)))
                {
                    using (var scroll = new EditorGUILayout.ScrollViewScope(_leftScroll))
                    {
                        _leftScroll = scroll.scrollPosition;
                        var sourceText = node.sourceText.Value;
                        ModifyTargetScript(_leftDetail, ref sourceText);
                        EditorGUILayout.TextArea(sourceText, _textStyle);
                    }
                }

                GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));
                using (var ver = new EditorGUILayout.VerticalScope(GUILayout.Width(width * 0.5f), GUILayout.ExpandHeight(true)))
                {
                    using (var scroll = new EditorGUILayout.ScrollViewScope(_rightScroll))
                    {
                        _rightScroll = scroll.scrollPosition;
                        ModifyTargetScript(_rightDetail,ref targetScript);
                        EditorGUILayout.TextArea(targetScript, _textStyle);
                    }
                }
            }
            EditorStyles.textField.wordWrap = val;
        }

        /// <summary>
        /// 重新构建目标脚本
        /// </summary>
        /// <param name="targetScript"></param>
        private void ModifyTargetScript(List<int> detail,ref string targetScript)
        {
            if (detail == null)
                return;

            var lines = targetScript.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (detail.Count > i)
                {
                    var state = detail[i];
                    switch (state)
                    {
                        case 1://新增加
                            lines[i] = "<color=green>" + lines[i] + "</color>";
                            break;
                        case 2://删除
                            lines[i] = "<color=red>" + lines[i] + "</color>";
                            break;
                        case 3://修改
                            lines[i] = "<color=yellow>" + lines[i] + "</color>";
                            break;
                        default:
                            break;
                    }
                }
            }
            targetScript = string.Join("\n", lines);
        }
    }
}
