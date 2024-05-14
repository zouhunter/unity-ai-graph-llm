/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 节点gui工具                                                                     *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;
using UFrame.NodeGraph.DataModel;
using UnityEngine.Events;

namespace UFrame.NodeGraph
{
    public class NodeView
    {
        protected Node _target;
        public virtual Node target { get { return _target; } set { _target = value; } }

        protected NodeBaseObjectDrawer targetDrawer;
        public virtual GUIStyle ActiveStyle { get { return EditorStyles.miniButton; } }
        public virtual GUIStyle InactiveStyle { get { return EditorStyles.miniButton; } }
        public virtual string Title => _target.name;
        public virtual void OnContextMenuGUI(GenericMenu menu, NodeGUI gui) { }
        public virtual float SuperHeight { get { return 0; } }

        public virtual float SuperWidth { get { return 0; } }

        public virtual void OnNodeGUI(Rect position, NodeData data)
        {
            DrawLabel(position, data);
        }

        protected virtual void DrawLabel(Rect position, NodeData data)
        {
            var oldColor = GUI.color;
            var textColor = (EditorGUIUtility.isProSkin) ? Color.black : oldColor;
            var style = new GUIStyle(EditorStyles.label);
            style.alignment = TextAnchor.MiddleCenter;
            var titleHeight = style.CalcSize(new GUIContent(Title)).y + NGEditorSettings.GUI.NODE_TITLE_HEIGHT_MARGIN;
            var nodeTitleRect = new Rect(0, 0, position.width, titleHeight);
            GUI.color = textColor;
            GUI.Label(nodeTitleRect, Title, style);
            GUI.color = oldColor;
        }

        public virtual void OnInspectorGUI(NodeGUI gui)
        {
            if (target == null) return;

            if (targetDrawer == null)
                targetDrawer = new NodeBaseObjectDrawer(target);

            targetDrawer.DrawHeader();
            targetDrawer.OnInspectorGUI();
        }

        public virtual void OnClickNodeGUI(NodeGUI nodeGUI, Vector2 mousePosition, ConnectionPointData result) { }

        protected void RecordUnDo(string message, NodeGUI node, bool saveOnScopeEnd, UnityAction action)
        {
            using (new RecordUndoScope("Change Node Name", node, saveOnScopeEnd))
            {
                action.Invoke();
            }
        }
    }
}

