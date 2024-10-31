/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2024-07-22                                                                   *
*  版本:                                                                *
*  功能:                                                                              *
*   - editor                                                                          *
*//************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;
using UnityEngine.UIElements;

namespace MateAI.ScriptableBehaviourTree
{
    public class BTreeWindow : EditorWindow
    {
        [OnOpenAsset(OnOpenAssetAttributeMode.Execute)]
        public static bool OnOpen(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is BTree tree)
            {
                GetWindow<BTreeWindow>().SelectBTree(tree);
                return true;
            }
            return false;
        }

        private BTree bTree;
        private ScrollViewContainer _scrollViewContainer;
        private GraphBackground _background;
        private Rect _graphRegion;
        private List<NodeView> _nodes;
        private List<KeyValuePair<NodeView, NodeView>> _connections;
        private List<BTree> _trees = new List<BTree>();
        private Rect _infoRegion;
        private bool _inConnect;
        private Vector2 _startConnectPos;
        private NodeView _activeNodeView;
        private Dictionary<BaseNode, string> _propPaths;
        private SerializedObject serializedObject;
        private List<BaseNode> _activeNodes;
        private Vector2 _variableScroll;
        private Rect _scrollContentSize;
        private float _splitRatio = 0.7f;
        private bool _isResizing;
        private void OnEnable()
        {
            _background = new GraphBackground("UI/Default");
            _graphRegion = position;
            _trees = new List<BTree>();
            _scrollViewContainer = new ScrollViewContainer();
            _scrollViewContainer.Start(rootVisualElement, GetGraphRegion());
            _scrollViewContainer.onGUI = DrawNodeGraphContent;
            RefreshGraphRegion();
            LoadNodeInfos();
            FindCached();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }
        private void OnUndoRedoPerformed()
        {
            LoadNodeInfos();
        }

        private void FindCached()
        {
            var treeIds = EditorPrefs.GetString("BTreeWindow.bTrees");
            if (!string.IsNullOrEmpty(treeIds))
            {
                var trees = treeIds.Split("|").Select(x => EditorUtility.InstanceIDToObject(int.Parse(x)) as BTree).ToList();
                trees.RemoveAll(x => !x);
                _trees.AddRange(trees);
            }
        }

        private Rect GetGraphRegion()
        {
            var graphRegion = new Rect(0, EditorGUIUtility.singleLineHeight, position.width * _splitRatio, position.height - 2 * EditorGUIUtility.singleLineHeight);
            return graphRegion;
        }

        private void LoadNodeInfos()
        {
            _nodes = new List<NodeView>();
            _connections = new List<KeyValuePair<NodeView, NodeView>>();
            if (bTree != null)
            {
                var rootTree = bTree.rootTree;
                var posMap = new Dictionary<TreeInfo, Vector2Int>();
                CalculateNodePositions(rootTree, 0, 0, posMap);
                MoveOffsetNodePostions(posMap);
                CreateViewDeepth(bTree, rootTree, rootTree, posMap, _nodes);
                var nodes = new List<BaseNode>();
                _propPaths = new Dictionary<BaseNode, string>();
                if (bTree is BVariantTree bvt)
                    bvt.BuildRootTree();
                BTreeDrawer.CollectNodeDeepth(bTree.rootTree, "_rootTree", nodes, _propPaths);
                serializedObject = new SerializedObject(bTree);
            }

            Repaint();
        }

        private NodeView CreateViewDeepth(BTree bTree, TreeInfo rootInfo, TreeInfo info, Dictionary<TreeInfo, Vector2Int> posMap, List<NodeView> _nodes)
        {
            var view = new NodeView(bTree, rootInfo, info, posMap[info]);
            view.onReload = LoadNodeInfos;
            view.onStartConnect = OnStartConnect;
            view.onActive = OnSetActiveView;
            this._nodes.Add(view);

            if (info.subTrees != null && info.subTrees.Count > 0)
            {
                for (var i = 0; i < info.subTrees.Count; ++i)
                {
                    var child = info.subTrees[i];
                    var childView = CreateViewDeepth(bTree, rootInfo, child, posMap, _nodes);
                    _connections.Add(new KeyValuePair<NodeView, NodeView>(view, childView));
                }
            }
            return view;
        }

        private void OnSetActiveView(NodeView nodeView)
        {
            _activeNodeView = nodeView;
        }

        private void OnStartConnect(Vector2 pos)
        {
            _startConnectPos = pos;
            _inConnect = true;
        }

        private void MoveOffsetNodePostions(Dictionary<TreeInfo, Vector2Int> posMap)
        {
            var posxArr = posMap.Values.Select(p => p.x).ToArray();
            var max = Mathf.Max(posxArr);
            var min = Mathf.Min(posxArr);
            var offset = Mathf.FloorToInt((max - min) * 0.5f);
            foreach (var node in posMap.Keys.ToArray())
            {
                var pos = posMap[node];
                pos.x -= offset;
                posMap[node] = pos;
            }
        }
        private void CalculateNodePositions(TreeInfo node, int depth, int offset, Dictionary<TreeInfo, Vector2Int> posMap)
        {
            Vector2Int pos = new Vector2Int(0, 0);
            int verticalSpacing = 10; // 垂直间距
            int horizontalSpacing = 10; // 水平间距

            pos.y = depth * (NodeView.MAX_HEIGHT + verticalSpacing);
            int subtreeOffset = offset;

            if (node.subTrees != null && node.subTrees.Count > 0)
            {
                foreach (var child in node.subTrees)
                {
                    CalculateNodePositions(child, depth + 1, subtreeOffset, posMap);
                    subtreeOffset += GetSubtreeWidth(child) + horizontalSpacing;
                }

                int firstChildX = posMap[node.subTrees[0]].x;
                int lastChildX = posMap[node.subTrees[node.subTrees.Count - 1]].x;
                pos.x = (firstChildX + lastChildX) / 2;
            }
            else
            {
                pos.x = offset + NodeView.WIDTH / 2;
            }

            posMap[node] = pos;
        }

        private int GetSubtreeWidth(TreeInfo node)
        {
            int horizontalSpacing = 10; // 水平间距
            int nodeWidth = NodeView.WIDTH; // 节点宽度
            if (node.subTrees == null || node.subTrees.Count == 0)
            {
                return nodeWidth; // 节点宽度
            }

            int width = 0;
            foreach (var child in node.subTrees)
            {
                width += GetSubtreeWidth(child) + horizontalSpacing; // 调整后的水平间距
            }

            return width - horizontalSpacing;
        }

        public void SelectBTree(BTree tree)
        {
            this.bTree = tree;
            LoadNodeInfos();
            EditorPrefs.SetInt("BTreeWindow.bTree", tree.GetInstanceID());
            if (!_trees.Contains(tree))
            {
                _trees.Add(tree);
                SaveTitleBTrees();
            }
        }

        private void SaveTitleBTrees()
        {
            _trees.RemoveAll(x => !x);
            EditorPrefs.SetString("BTreeWindow.bTrees", string.Join("|", _trees.Select(x => x.GetInstanceID()).ToArray()));
        }

        private void OnGUI()
        {
            DrawVerticalLine();
            DrawSelectTree();
            if (!bTree)
            {
                bTree = EditorUtility.InstanceIDToObject(EditorPrefs.GetInt("BTreeWindow.bTree")) as BTree;
                if (bTree)
                    SelectBTree(bTree);
                else
                    return;
            }
            _background?.Draw(_graphRegion, _scrollViewContainer.scrollOffset, _scrollViewContainer.ZoomSize);
            RefreshGraphRegion();
            DrawInformations();
            DrawFootPrint();
        }

        private void DrawVerticalLine()
        {
            float leftWidth = position.width * _splitRatio;
            var verticalSplitterRect = new Rect(leftWidth, 0, _isResizing ? 5 : 2, position.height);
            EditorGUI.DrawRect(verticalSplitterRect, _isResizing ? Color.gray : Color.black);
            verticalSplitterRect.width = 5;
            HandleSplitterDrag(verticalSplitterRect);
        }

        private void DrawSelectTree()
        {
            EditorGUI.LabelField(new Rect(10, 0, 50, EditorGUIUtility.singleLineHeight), "[Trees]:", EditorStyles.boldLabel);
            var rect = new Rect(60, 0, 120, EditorGUIUtility.singleLineHeight);
            for (int i = 0; i < _trees.Count; i++)
            {
                var boxRect = new Rect(rect.x, rect.y, rect.width + 20, rect.height);
                GUI.Box(boxRect, "");
                var active = _trees[i] == bTree;
                var style = active ? EditorStyles.toolbarDropDown : EditorStyles.toolbarButton;
                if (_trees[i] == null)
                    continue;

                if (EditorGUI.ToggleLeft(rect, _trees[i].name, active, style) && !active)
                {
                    SelectBTree(_trees[i]);
                    active = true;
                }
                var btn = new Rect(rect.max.x, 0, 15, 15);
                if (_trees.Count > 1 && GUI.Button(btn, "x"))
                {
                    _trees.RemoveAt(i);
                    if (active)
                    {
                        SelectBTree(_trees[0]);
                    }
                    SaveTitleBTrees();
                    break;
                }
                rect.x += rect.width + 20;
            }
        }

        private void RefreshGraphRegion()
        {
            var graphRegion = GetGraphRegion();
            if (_scrollViewContainer != null && _graphRegion != graphRegion)
            {
                _graphRegion = graphRegion;
                _scrollViewContainer.UpdateScale(graphRegion);
                _scrollViewContainer.Refesh();
            }
            _scrollViewContainer.ApplyOffset();
        }

        private void DrawConnections()
        {
            if (_connections == null)
                return;

            // 绘制连接线
            foreach (var connection in _connections)
            {
                var start = connection.Key;
                var end = connection.Value;
                DrawNodeCurve(start, end);
            }
        }

        void DrawNodeCurve(NodeView startView, NodeView endView)
        {
            if (_scrollViewContainer == null)
                return;
            var active = startView.Info.enable && endView.Info.enable;
            var start = new Vector2(startView.Rect.center.x + 2, startView.Rect.max.y - 2);
            var end = new Vector2(endView.Rect.center.x + 2, endView.Rect.min.y + 2);
            var color = Color.white;
            if (!active)
            {
                color = Color.gray;
            }
            else
            {
                color = GraphColorUtil.GetColorByState(endView.Info.status, endView.Info.tickCount == bTree.TickCount);
            }
            DrawCurve(start, end, color);
        }

        void DrawCurve(Vector2 start, Vector2 end, Color color)
        {
            Handles.BeginGUI();
            Vector2 startTan = start + Vector2.up * 50; // 控制点向右延伸50像素
            Vector2 endTan = end + Vector2.down * 50; // 控制点向左延伸50像素
            Handles.DrawBezier(start, end, startTan, endTan, color, null, 2);
            Handles.EndGUI();
        }

        private void DrawNodeGraphContent()
        {
            var contentRect = new Rect(Vector2.zero, _graphRegion.size / _scrollViewContainer.minZoomSize);
            BeginWindows();
            _nodes?.ForEach(node =>
            {
                node.DrawNode(contentRect);
            });
            HandleDragNodes();
            EndWindows();
            DrawConnections();
        }

        private void HandleDragNodes()
        {
            if (_inConnect && Event.current.type == EventType.MouseUp)
            {
                _inConnect = false;
                NodeView targetView = null;
                foreach (var node in _nodes)
                {
                    if (node.Rect.Contains(Event.current.mousePosition))
                    {
                        targetView = node;
                        break;
                    }
                }
                if (targetView == null || targetView == _activeNodeView)
                {
                    RecordUndo("add child!");
                    _activeNodeView?.CreateAndAddChild();
                }
                else if (CheckAsChildAble(_activeNodeView.Info, targetView.Info))
                {
                    var oldParent = targetView.RefreshParentNode();
                    if (oldParent != null)
                    {
                        RecordUndo("modify parent!");
                        oldParent.subTrees.Remove(targetView.Info);
                        _activeNodeView.Info.subTrees.Add(targetView.Info);
                        LoadNodeInfos();
                    }
                }
            }

            if (_inConnect)
            {
                var start = _startConnectPos;
                var end = Event.current.mousePosition;
                DrawCurve(start, end, Color.cyan);
            }
        }

        private bool CheckAsChildAble(TreeInfo parent, TreeInfo child)
        {
            if (parent == null || child == null)
                return false;

            if (child == parent) return false;

            if (child.subTrees != null && child.subTrees.Count > 0)
            {
                foreach (var subTree in child.subTrees)
                {
                    if (subTree == parent)
                        return false;
                    if (!CheckAsChildAble(parent, subTree))
                        return false;
                }
            }
            return true;
        }


        private void RecordUndo(string message)
        {
            Undo.RecordObject(bTree, message);
        }


        private void DrawFootPrint()
        {
            var region = new Rect(0, position.height - EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(region, "zouhangte@wekoi.cn,power by uframe", EditorStyles.linkLabel))
            {
                Application.OpenURL("https://alidocs.dingtalk.com/i/nodes/gvNG4YZ7Jne60YR3f2jYBoPyV2LD0oRE?doc_type=wiki_doc&orderType=SORT_KEY&rnd=0.48325224514433396&sortType=DESC");
            }
        }

        private void DrawTitleInfo(Rect contentRect)
        {
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel);
            centeredStyle.alignment = TextAnchor.MiddleLeft;
            centeredStyle.fontSize = 14; // 设置字体大小
            centeredStyle.richText = true;
            var name = bTree != null ? bTree.name : "(none)";
            var readMeRect = new Rect(contentRect.x + contentRect.width - 60, contentRect.max.y - 20, 60, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.LinkButton(readMeRect, "README"))
                Application.OpenURL("https://web-alidocs.dingtalk.com/i/nodes/QOG9lyrgJP3BQREofr345nLxVzN67Mw4?doc_type=wiki_doc");
            GUIContent content = new GUIContent($"Unity-动作行为树 <size=12><b><color=black>v1.0</color></b></size> <size=12><b>({name})</b></size>");
            if (GUI.Button(contentRect, content, centeredStyle) && bTree != null)
            {
                EditorGUIUtility.PingObject(bTree);
            }
            var lineRect = new Rect(contentRect.x, contentRect.y + contentRect.height, contentRect.width, 3);
            GUI.Box(lineRect, "");
        }

        private void CollectNodes()
        {
            _activeNodes = _activeNodes ?? new List<BaseNode>();
            _activeNodes.Clear();
            if (_activeNodeView != null)
            {
                if (_activeNodeView.Info.node)
                    _activeNodes.Add(_activeNodeView.Info.node);
                if (_activeNodeView.Info.condition.conditions != null)
                {
                    foreach (var condition in _activeNodeView.Info.condition.conditions)
                    {
                        _activeNodes.Add(condition.node);
                        if (condition.subConditions != null)
                        {
                            foreach (var item in condition.subConditions)
                            {
                                _activeNodes.Add(item.node);
                            }
                        }
                    }
                }
            }
        }
        private void DrawNodeProps(ref Rect rect)
        {
            serializedObject?.Update();
            int i = 1;
            foreach (var node in _activeNodes)
            {
                if (!node)
                    continue;
                float startY = rect.y;
                if (node)
                {
                    var titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 12,
                        alignment = TextAnchor.MiddleLeft,
                        normal = { textColor = Color.white * 0.6f }
                    };
                    EditorGUI.DrawTextureTransparent(rect, EditorGUIUtility.IconContent("gameviewbackground@2x").image);
                    GUI.Button(rect, i++.ToString() + "," + _activeNodeView.Info.node.GetType().FullName, titleStyle);
                    rect.y += EditorGUIUtility.singleLineHeight + 2;
                }

                var nodeItemRect = rect;
                nodeItemRect.x += 20;
                nodeItemRect.width -= 20;
                TreeInfoDrawer.DrawCreateNodeContent(nodeItemRect, node, (x) =>
                {
                    _activeNodeView.Info.node = x;
                }, bTree);

                if (node && _propPaths.TryGetValue(node, out var path))
                {
                    var drawer = serializedObject.FindProperty(path);
                    if (drawer != null)
                    {
                        var labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 80;
                        rect.height = EditorGUI.GetPropertyHeight(drawer, true);
                        EditorGUI.PropertyField(rect, drawer, GUIContent.none, true);
                        EditorGUIUtility.labelWidth = labelWidth;
                        rect.y += rect.height;
                    }
                }
                var endY = rect.y + rect.height;
                rect.y += 30;
                rect.height = EditorGUIUtility.singleLineHeight;
            }
            serializedObject?.ApplyModifiedProperties();
        }

        private void DrawInformations()
        {
            _infoRegion = new Rect(position.width * _splitRatio + 5, EditorGUIUtility.singleLineHeight, position.width * (1-_splitRatio) - 5, position.height - 2 * EditorGUIUtility.singleLineHeight);
            var contentRect = new Rect(_infoRegion.x, _infoRegion.y, _infoRegion.width, 2 * EditorGUIUtility.singleLineHeight);
            DrawTitleInfo(contentRect);
            var infoRect = new Rect(_infoRegion.x, contentRect.yMax + 5, _infoRegion.width, _infoRegion.height - EditorGUIUtility.singleLineHeight * 2);
            var rect = new Rect(infoRect.x, infoRect.y, infoRect.width, EditorGUIUtility.singleLineHeight);
            if (_activeNodeView != null && _activeNodeView.Info != null)
            {
                var descRect = new Rect(_infoRegion.x + 10, rect.yMax, _infoRegion.width - 20, EditorGUIUtility.singleLineHeight * 3);
                _activeNodeView.Info.desc = EditorGUI.TextArea(descRect, _activeNodeView.Info.desc);
                if (string.IsNullOrEmpty(_activeNodeView.Info.desc))
                {
                    descRect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.LabelField(descRect, "Description...", EditorStyles.centeredGreyMiniLabel);
                }
                rect.y += EditorGUIUtility.singleLineHeight * 5;
                CollectNodes();
                if (_activeNodes.Count > 0)
                {
                    DrawNodeProps(ref rect);
                }
            }
            var height = _infoRegion.height - (rect.yMax - contentRect.y);
            height = Mathf.Min(height, _infoRegion.height * 0.5f);
            var scrollRect = new Rect(_infoRegion.x, _infoRegion.y + _infoRegion.height - height, _infoRegion.width, height);
            DrawVariables(scrollRect);
        }

        private void DrawVariables(Rect rect)
        {
            var titleRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight * 0.8f);
            GUI.Box(titleRect, "");
            EditorGUI.LabelField(titleRect, "变量集合:", EditorStyles.boldLabel);

            rect.y += EditorGUIUtility.singleLineHeight;
            rect.height -= EditorGUIUtility.singleLineHeight;
            _scrollContentSize.width = rect.width - 20;
            var fieldHeight = 0f;
            GUI.Box(rect, "");
            _variableScroll = GUI.BeginScrollView(rect, _variableScroll, _scrollContentSize);
            var index = 1;
            using (var liter = bTree.Variables.GetGetEnumerator())
            {
                while (liter.MoveNext())
                {
                    var lineHeight = EditorGUIUtility.singleLineHeight + 4;
                    fieldHeight += lineHeight;
                    using (var hori = new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorGUIUtility.singleLineHeight + 4)))
                    {
                        EditorGUILayout.LabelField(index++.ToString("00"),EditorStyles.miniBoldLabel,GUILayout.Width(20));
                        var widthLabel = _scrollContentSize.width * 0.4f;
                        EditorGUILayout.LabelField(liter.Current.Key + " : ", EditorStyles.miniBoldLabel,GUILayout.Width(widthLabel));
                        var value = liter.Current.Value.GetValue();
                        var layout = GUILayout.Height(EditorGUIUtility.singleLineHeight);
                        if (value == null)
                        {
                            var contentType = liter.Current.Value.GetType();
                            if (contentType.IsGenericType)
                            {
                               var subType = contentType.GetGenericArguments()[0];
                                EditorGUILayout.LabelField($"Null ({subType.Name})", layout);
                            }
                            else
                            {
                                EditorGUILayout.LabelField("Null", layout);
                            }
                            continue;
                        }
                        var type = value.GetType();
                        if (value is UnityEngine.Object o)
                        {
                            EditorGUILayout.ObjectField(o, o.GetType(), false, layout);
                        }
                        else if (value is Vector2 v2)
                        {
                            EditorGUILayout.Vector2Field("", v2, layout);
                        }
                        else if (value is Vector3 v3)
                        {
                            EditorGUILayout.Vector3Field("", v3, layout);
                        }
                        else if (value is Vector4 v4)
                        {
                            EditorGUILayout.Vector4Field("", v4, layout);
                        }
                        else if (value is Rect r)
                        {
                            EditorGUILayout.RectField("", r, layout);
                        }
                        else
                        {
                            EditorGUILayout.LabelField(value.ToString() + $" ({type.Name})",EditorStyles.textField, layout);
                        }
                    }
                }
            }
            _scrollContentSize.height = Mathf.Max(_scrollContentSize.height, rect.height, fieldHeight);
            GUI.EndScrollView();
        }
     
        private void HandleSplitterDrag(Rect verticalSplitterRect)
        {
            EditorGUIUtility.AddCursorRect(verticalSplitterRect, MouseCursor.ResizeHorizontal);
            if (_isResizing &&(Event.current.button == 1 || Event.current.type == EventType.MouseUp))
            {
                _isResizing = false;
                Event.current.Use(); // 使用事件，防止传播
            }
            else if (_isResizing)
            {
                float mouseX = Event.current.mousePosition.x;
                _splitRatio = Mathf.Clamp(mouseX / position.width, 0.1f, 0.9f);
                Repaint();
            }
            else if (Event.current.type == EventType.MouseDown && verticalSplitterRect.Contains(Event.current.mousePosition))
            {
                _isResizing = true;
                Event.current.Use(); // 使用事件，防止传播
            }
        }
    }
}
