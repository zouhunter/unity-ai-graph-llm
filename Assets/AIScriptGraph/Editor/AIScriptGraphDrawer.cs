using NUnit.Framework.Interfaces;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UFrame.NodeGraph.DataModel;

using UnityEditor;
using UnityEditor.MemoryProfiler;

using UnityEditorInternal;

using UnityEngine;

namespace AIScripting
{
    [CustomEditor(typeof(AIScriptGraph))]
    public class AIScriptGraphDrawer : Editor
    {
        private ReorderableList _nodeList;
        private ReorderableList _connectionList;
        private Dictionary<UnityEngine.Object, NodeBaseInfoDrawer> _nodesDrawers;
        private void OnEnable()
        {
            _nodeList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_allNodes"), true, true, true, true);
            _connectionList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_allConnections"), true, true, true, true);

            _nodeList.drawHeaderCallback = OnNodeListHead;
            _nodeList.elementHeightCallback = OnNodeElementHeight;
            _nodeList.drawElementCallback = OnNodeListElement;

            _connectionList.drawHeaderCallback = OnConnectionListHead;
            _connectionList.drawElementCallback = OnConnectionListElement;
            _connectionList.elementHeightCallback = OnConnectionElementHeight;
            _nodesDrawers = new Dictionary<UnityEngine.Object, NodeBaseInfoDrawer>();
        }

        private void OnConnectionListHead(Rect rect)
        {
            EditorGUI.LabelField(rect, "Connection List");
        }


        private NodeBaseInfoDrawer GetDrawer(SerializedProperty prop,string name)
        {
            var targetProp = prop?.FindPropertyRelative(name);
            if (!_nodesDrawers.TryGetValue(targetProp.objectReferenceValue, out var drawer))
            {
                drawer = _nodesDrawers[targetProp.objectReferenceValue] = new NodeBaseInfoDrawer(targetProp.objectReferenceValue as NodeBaseObject);
            }
            return drawer;
        }

        private float OnConnectionElementHeight(int index)
        {
            var element = _connectionList.serializedProperty.GetArrayElementAtIndex(index);
            var drawer = GetDrawer(element, "m_connection");
            return drawer.GetHeight();
        }

        private void OnConnectionListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _connectionList.serializedProperty.GetArrayElementAtIndex(index);
            var drawer = GetDrawer(element, "m_connection");

            drawer.OnGUI(rect);
        }

        private void OnNodeListHead(Rect rect)
        {
            EditorGUI.LabelField(rect, "Node List");
        }

        private void OnNodeListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _nodeList.serializedProperty.GetArrayElementAtIndex(index);
            var drawer = GetDrawer(element, "m_node");
            drawer.OnGUI(rect);
        }

        private float OnNodeElementHeight(int index)
        {
            var element = _nodeList.serializedProperty.GetArrayElementAtIndex(index);
            var drawer = GetDrawer(element, "m_node");
            return drawer.GetHeight();
        }

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
        }
        public override void OnInspectorGUI()
        {
            DrawTitleContent();
            _nodeList.DoLayoutList();
            _connectionList.DoLayoutList();
        }

        private void DrawTitleContent()
        {
            GUILayout.Space(25);
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel);
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            centeredStyle.fontSize = 18; // 设置字体大小
            GUILayout.Label("AI编辑器 v1.0.0", centeredStyle);

            var lastRect = GUILayoutUtility.GetLastRect();
            var readMeRect = new Rect(lastRect.x + lastRect.width - 60, lastRect.max.y - 20, 60, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(readMeRect, "README", EditorStyles.miniButtonRight))
            {
                Application.OpenURL("https://alidocs.dingtalk.com/i/nodes/ydxXB52LJq76yxY1tyg50kKOWqjMp697?utm_scene=team_space");
            }
            // 添加分割线
            GUILayout.Box("", GUILayout.Height(3), GUILayout.ExpandWidth(true));
        }
    }
}
