using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UFrame.NodeGraph.DataModel;

using UnityEditor;
using UnityEditor.MemoryProfiler;

using UnityEditorInternal;

using UnityEngine;
using UnityEngine.UIElements;

namespace AIScripting
{
    [CustomEditor(typeof(AIScriptGraph))]
    public class AIScriptGraphDrawer : Editor
    {
        private ReorderableList _nodeList;
        private ReorderableList _connectionList;
        private Dictionary<UnityEngine.Object, NodeBaseInfoDrawer> _nodesDrawers;
        private Vector2 offset;

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
            if (targetProp.objectReferenceValue == null)
                return null;

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
            return drawer?.GetHeight() ?? 0;
        }

        private void OnConnectionListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _connectionList.serializedProperty.GetArrayElementAtIndex(index);
            var drawer = GetDrawer(element, "m_connection");
            drawer?.OnGUI(rect);
        }

        private void OnNodeListHead(Rect rect)
        {
            EditorGUI.LabelField(rect, "Node List");
        }

        private void OnNodeListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _nodeList.serializedProperty.GetArrayElementAtIndex(index);
            var drawer = GetDrawer(element, "m_node");
            
            drawer?.OnGUI(rect);
            if (drawer != null && drawer.expand)
            {
                //rect.height -= EditorGUIUtility.singleLineHeight + 4;
                DrawGridBox(rect);
            }

        }

        private float OnNodeElementHeight(int index)
        {
            var element = _nodeList.serializedProperty.GetArrayElementAtIndex(index);
            var drawer = GetDrawer(element, "m_node");
            return drawer?.GetHeight()??0;
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

        private void DrawGridBox(Rect rect)
        {
            DrawGrid(rect, 20, 0.2f, Color.gray);
            DrawGrid(rect, 100, 0.4f, Color.gray);
        }

        private void DrawTitleContent()
        {
            GUILayout.Space(25);
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel);
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            centeredStyle.fontSize = 18; // 设置字体大小
            centeredStyle.richText = true;
            //GUILayout.Label("AI状态机 v1.0", centeredStyle);
            GUIContent content = new GUIContent("AI状态机 <size=12><b><color=black>v1.0</color></b></size>");
            EditorGUILayout.LabelField(content, centeredStyle);

            var lastRect = GUILayoutUtility.GetLastRect();
            var readMeRect = new Rect(lastRect.x + lastRect.width - 60, lastRect.max.y - 20, 60, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.LinkButton(readMeRect, "README"))
            {
                Application.OpenURL("https://alidocs.dingtalk.com/i/nodes/");
            }
            // 添加分割线
            GUILayout.Box("", GUILayout.Height(3), GUILayout.ExpandWidth(true));
        }

        private void DrawGrid(Rect rect,float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(rect.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(rect.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            Vector3 offset = rect.position;
            Vector3 start = new Vector3(rect.x, rect.y, 0);
            Vector3 end = new Vector3(rect.x + rect.width, rect.y + rect.height, 0);
            var yMin = Mathf.Clamp(-gridSpacing + offset.y, start.y, end.y);
            var yMax = Mathf.Clamp(rect.height + offset.y, start.y, end.y);
            var xMin = Mathf.Clamp(-gridSpacing + offset.x, start.x, end.x);
            var xMax = Mathf.Clamp(rect.width + offset.x, start.x, end.x);

            for (int i = 0; i < widthDivs; i++)
            {
                var x = gridSpacing * i + offset.x;
                Handles.DrawLine(new Vector3(x, yMin),new Vector3(x, yMax));
            }

            for (int j = 0; j < heightDivs; j++)
            {
                var y = gridSpacing * j;
                Handles.DrawLine(new Vector3(xMin,y),new Vector3(xMax,y));
            }
            Handles.color = Color.white;
            Handles.EndGUI();
        }

    }
}
