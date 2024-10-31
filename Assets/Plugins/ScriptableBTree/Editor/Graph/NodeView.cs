/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2024-07-22                                                                   *
*  版本:                                                                *
*  功能:                                                                              *
*   - editor                                                                          */

using System;

using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    [Serializable]
    public class NodeView
    {
        private int _windowId;
        private BTree _tree;
        private TreeInfo _rootInfo;
        private TreeInfo _info;
        private TreeInfo _parentInfo;
        private Vector2 _position;
        private Rect _rect;
        public Rect Rect => _rect;
        public TreeInfo Info => _info;
        private ConditionDrawer _conditionDrawer;
        private bool _rectInited;
        public const int WIDTH = 250;
        public const int MIN_HEIGHT = 60;
        public const int MAX_HEIGHT = 250;
        private bool _forceAddChild;
        public Action onReload { get; set; }
        public Action<NodeView> onActive { get; set; }
        public Action<Vector2> onStartConnect { get; set; }
        public NodeView(BTree bTree, TreeInfo rootInfo, TreeInfo info, Vector2 pos)
        {
            this._tree = bTree;
            this._rootInfo = rootInfo;
            this._info = info;
            this._position = pos;
            this._windowId = info.GetHashCode();
            _conditionDrawer = new ConditionDrawer(_tree, info);
        }

        internal void DrawNode(Rect contentRect)
        {
            if (!_rectInited)
            {
                var pos = _position;
                _rect = new Rect(pos.x, pos.y, WIDTH, GetHeight());
                _rect.position = _rect.position + new Vector2(contentRect.center.x, WIDTH);
                _rectInited = true;
            }
            _rect.height = GetHeight();
            _rect = GUI.Window(_windowId, _rect, DrawThisNode, "");
        }

        private float GetHeight()
        {
            float height = MIN_HEIGHT;
            if (_info.condition.enable)
            {
                var conditionHeight = _conditionDrawer.GetHeight();
                height += conditionHeight;
            }
            return height;
        }

        private TreeInfo FindParentNode(TreeInfo info, TreeInfo match)
        {
            if (info.subTrees == null)
                return null;

            if (info.subTrees.Contains(match))
            {
                return info;
            }
            else
            {
                foreach (var subInfo in info.subTrees)
                {
                    var parent = FindParentNode(subInfo, match);
                    if (parent != null)
                    {
                        return parent;
                    }
                }
            }
            return null;
        }

        public TreeInfo RefreshParentNode()
        {
            if (_rootInfo == _info)
                return null;

            if (_parentInfo == null || _parentInfo.subTrees == null || !_parentInfo.subTrees.Contains(_info))
            {
                _parentInfo = FindParentNode(_rootInfo, _info);
            }
            return _parentInfo;
        }

        private bool CheckNeedGreen(TreeInfo info)
        {
            if (EditorApplication.isPlaying)
                return false;

            if (_tree is BVariantTree variantTree)
            {
                var baseInfo = variantTree.baseTree.FindTreeInfo(info.id);
                if (baseInfo == null)
                {
                    return true;
                }
                if (baseInfo.enable != info.enable || baseInfo.id != info.id)
                {
                    return true;
                }
                if (baseInfo.condition.enable != info.condition.enable)
                {
                    return true;
                }
                if (baseInfo.condition.matchType != info.condition.matchType)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            return false;
        }

        private void DrawThisNode(int id)
        {
            var rect = new Rect(0, 0, _rect.width, _rect.height);

            if (_rootInfo != _info)
            {
                var upPortRect = new Rect(_rect.width * 0.5f - 6, 2, 12, 12);
                _info.enable = EditorGUI.Toggle(upPortRect, _info.enable, EditorStyles.radioButton);
            }
            var typeRect = new Rect(rect.x + 2, rect.y, rect.width - 4, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(typeRect, _info.node?.GetType()?.Name?.ToString(), EditorStyles.centeredGreyMiniLabel);

            RefreshParentNode();
            DrawMoveLeftRight(rect);
            var needGreen = CheckNeedGreen(_info);

            using (var disableScope = new EditorGUI.DisabledScope(!_info.enable))
            {
                var nodeRect = new Rect(5, 20, _rect.width - 40, EditorGUIUtility.singleLineHeight);
                using(var colorScope = new ColorScope(needGreen, Color.green))
                {
                    TreeInfoDrawer.DrawCreateNodeContent(nodeRect, _info.node, n =>
                    {
                        RecordUndo("node changed!");
                        _info.node = n;
                    }, _tree);
                    DrawNodeState(_info.status,nodeRect);
                }
                var conditionEnableRect = new Rect(nodeRect.x + nodeRect.width + 10, nodeRect.y, 20, 20);
                _info.condition.enable = EditorGUI.Toggle(conditionEnableRect, _info.condition.enable, EditorStyles.radioButton);

                if (_info.condition.enable)
                {
                    var height = _conditionDrawer.GetHeight();
                    var conditonRect = new Rect(nodeRect.x + 10, nodeRect.y + EditorGUIUtility.singleLineHeight, _rect.width - 20, height);
                    _conditionDrawer.OnGUI(conditonRect);
                }
                if ((_info.subTrees != null && _info.subTrees.Count > 0) || (_info.node && _info.node is ParentNode) || _forceAddChild)
                {
                    var downPortRect = new Rect(_rect.width * 0.5f - 6, _rect.height - 16, 12, 12);
                    TryStartDrawLine(downPortRect);
                    EditorGUI.Toggle(downPortRect, true, EditorStyles.radioButton);
                }
                if (!string.IsNullOrEmpty(_info.desc)) {
                    var descRect = new Rect(10, rect.height - 16, rect.width - 20, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(descRect, _info.desc, EditorStyles.miniLabel);
                }
            }
            if (rect.Contains(Event.current.mousePosition))
            {
                ProcessEvents();
            }
            GUI.DragWindow(new Rect(0, 0, _rect.width, _rect.height));
        }

        private void TryStartDrawLine(Rect rect)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    onActive?.Invoke(this);
                    onStartConnect?.Invoke(_rect.position + rect.center);
                }
            }
        }

        private void DeleteSelf()
        {
            if (_parentInfo != null)
            {
                RecordUndo("delete child!");
                _info.subTrees?.ForEach(sub =>
                {
                    _parentInfo.subTrees.Add(sub);
                });
                _info.subTrees = new System.Collections.Generic.List<TreeInfo>();
                _parentInfo.subTrees.Remove(_info);
                onReload?.Invoke();
            }
        }

        private void AddCopyChild(TreeInfo copyFrom)
        {
            var childInfo = new TreeInfo();
            childInfo.id = System.Guid.NewGuid().ToString();
            CopyPasteUtil.CopyTreeInfo(copyFrom, childInfo, childInfo);
            _info.subTrees.Add(childInfo);
            onReload.Invoke();
        }

        private void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                if(_info != null && _info.node != null && !(_info.node is ParentNode) && Event.current.control)
                {
                    _forceAddChild = !_forceAddChild;
                }
                onActive?.Invoke(this);
            }
            if (Event.current.keyCode == KeyCode.Delete && Event.current.type == EventType.KeyUp)
            {
                DeleteSelf();
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.D && Event.current.type == EventType.KeyDown)
            {
                RecordUndo("double");
                AddCopyChild(_info);
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.C && Event.current.type == EventType.KeyDown)
            {
                CopyPasteUtil.copyedTreeInfo = _info;
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.X && Event.current.type == EventType.KeyDown)
            {
                CopyPasteUtil.copyedTreeInfo = _info;
                DeleteSelf();
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.V && Event.current.type == EventType.KeyDown)
            {
                if (CopyPasteUtil.copyedTreeInfo != null)
                {
                    RecordUndo("add child");
                    AddCopyChild(CopyPasteUtil.copyedTreeInfo);
                }
            }

        }

        private void DrawMoveLeftRight(Rect rect)
        {
            if (_tree is BVariantTree)
                return;

            if (_parentInfo != null && _parentInfo.subTrees != null)
            {
                var index = _parentInfo.subTrees.IndexOf(_info);
                var drawMoveLeft = index != 0;
                var drawMoveRight = index != _parentInfo.subTrees.Count - 1;
                if (drawMoveLeft)
                {
                    var moveLeftRect = new Rect(rect.x + 3, rect.y, 20, 20);
                    if (GUI.Button(moveLeftRect, EditorGUIUtility.IconContent("d_scrollleft")))
                    {
                        var left = _parentInfo.subTrees[index - 1];
                        _parentInfo.subTrees[index - 1] = _info;
                        _parentInfo.subTrees[index] = left;
                        onReload?.Invoke();
                    }
                }
                if (drawMoveRight)
                {
                    var moveRightRect = new Rect(rect.x + rect.width - 23, rect.y, 20, 20);
                    if (GUI.Button(moveRightRect, EditorGUIUtility.IconContent("d_scrollright")))
                    {
                        var right = _parentInfo.subTrees[index + 1];
                        _parentInfo.subTrees[index + 1] = _info;
                        _parentInfo.subTrees[index] = right;
                        onReload?.Invoke();
                    }
                }
            }

        }


        private void DrawNodeState(Status status, Rect rect)
        {
            var color = Color.gray;
            var show = false;
            if (status == Status.Success)
            {
                show = true;
                color = Color.green;
            }
            else if (status == Status.Failure)
            {
                show = true;
                color = Color.red;
            }
            else if (status == Status.Running)
            {
                show = true;
                color = Color.yellow;
            }
            if (show)
            {
                using (var colorScope = new ColorGUIScope(true, color))
                {
                    GUI.Box(rect, "");
                }
            }
        }

        private void RecordUndo(string message)
        {
            Undo.RecordObject(_tree, message);
        }

        public void CreateAndAddChild()
        {
            CreateNodeWindow.Show(Event.current.mousePosition, (node) =>
            {
                var childInfo = new TreeInfo();
                childInfo.id = System.Guid.NewGuid().ToString();
                childInfo.enable = true;
                childInfo.node = node;
                if (_info.subTrees == null)
                    _info.subTrees = new List<TreeInfo>();
                _info.subTrees.Add(childInfo);
                onReload.Invoke();
            }, typeof(BaseNode), _tree);
        }
    }
}
