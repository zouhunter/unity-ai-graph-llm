/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-13
 * Version: 1.0.0
 * Description: 
 *_*/
using MateAI.ScriptableBehaviourTree.Actions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    public class TreeInfoDrawer
    {
        private BTreeDrawer _treeDrawer;
        private TreeInfoDrawer _parentTree;
        private TreeInfo _treeInfo;
        private Dictionary<TreeInfo, TreeInfoDrawer> _subDrawers;
        private ReorderableList _subTreeList;
        private bool _changed;
        private string _deepth;
        private BTree _tree;
        private ConditionDrawer _conditionDrawer;
        private Dictionary<int, ReorderableList> _subConditionListMap = new Dictionary<int, ReorderableList>();
        public bool changed
        {
            get
            {
                if (_changed)
                {
                    _changed = false;
                    return true;
                }
                if (_subDrawers != null)
                {
                    foreach (var pair in _subDrawers)
                    {
                        if (pair.Value.changed)
                            return true;
                    }
                }
                return false;
            }
        }
        public BaseNode node => _treeInfo == null ? null : _treeInfo.node;

        public TreeInfoDrawer(BTreeDrawer treeDrawer, TreeInfoDrawer parentTree, TreeInfo info)
        {
            _treeDrawer = treeDrawer;
            _treeInfo = info;
            _tree = (treeDrawer.target as BTree);
            _parentTree = parentTree;
            RebuildSubTreeList();
            RebuildConditionList();
        }

        private void RebuildConditionList()
        {
            if (_treeInfo == null)
                return;

            _treeInfo.condition = _treeInfo.condition ?? new ConditionInfo();
            _treeInfo.condition.conditions = _treeInfo.condition.conditions ?? new List<ConditionItem>();
            _conditionDrawer = new ConditionDrawer(_treeDrawer.target as BTree, _treeInfo);
        }

        private void RebuildSubTreeList()
        {
            if (_treeInfo == null)
                return;

            _subDrawers = new Dictionary<TreeInfo, TreeInfoDrawer>();
            _treeInfo.subTrees = _treeInfo.subTrees ?? new List<TreeInfo>();
            foreach (var child in _treeInfo.subTrees)
            {
                if (child == null) continue;
                _subDrawers[child] = new TreeInfoDrawer(_treeDrawer, this, child);
            }
            bool canAddOrDelete = !(_treeDrawer.target is BVariantTree);
            _subTreeList = new ReorderableList(_treeInfo.subTrees, typeof(TreeInfo), true, true, canAddOrDelete, canAddOrDelete);
            _subTreeList.headerHeight = 0;
            _subTreeList.elementHeightCallback = OnSubTreeElementHeight;
            _subTreeList.drawElementCallback = OnDrawSubTreeElement;
            _subTreeList.onAddCallback = OnAddSubTreeElement;
            _subTreeList.onRemoveCallback = OnRemoveSubTreeElement;
        }

        private void OnRemoveSubTreeElement(ReorderableList list)
        {
            var baseTreeInfo = TreeInfoInBase(_treeInfo);
            if (baseTreeInfo != null)
            {
                EditorUtility.DisplayDialog("Error", "Can't delete base tree sub node", "OK");
                return;
            }

            RecordUndo("remove sub tree element");
            _treeInfo.subTrees.RemoveAt(list.index);
            _changed = true;
        }

        private void OnAddSubTreeElement(ReorderableList list)
        {
            var baseTreeInfo = TreeInfoInBase(_treeInfo);
            if (baseTreeInfo != null)
            {
                EditorUtility.DisplayDialog("Error", "Can't add base tree sub node", "OK");
                return;
            }

            RecordUndo("add sub tree element");
            if (_treeInfo.node is ParentNode)
            {
                var maxChildCount = (_treeInfo.node as ParentNode).maxChildCount;
                if (_treeInfo.subTrees != null && maxChildCount <= _treeInfo.subTrees.Count)
                    return;
            }
            var treeInfo = new TreeInfo();
            treeInfo.enable = true;
            treeInfo.id = Guid.NewGuid().ToString();
            _treeInfo.subTrees.Add(treeInfo);
            _subDrawers[treeInfo] = new TreeInfoDrawer(_treeDrawer, this, treeInfo);
            _changed = true;
        }

        private TreeInfoDrawer GetSubDrawer(TreeInfo info)
        {
            if (!_subDrawers.TryGetValue(info, out var drawer))
            {
                drawer = new TreeInfoDrawer(_treeDrawer, this, info);
                _subDrawers[info] = drawer;
            }
            return drawer;
        }

        private float OnSubTreeElementHeight(int index)
        {
            var elementHeight = 4;
            if (_treeInfo.subTrees.Count > index)
            {
                var info = _treeInfo.subTrees[index];
                var drawer = GetSubDrawer(info);
                return drawer.GetHeight() + elementHeight;
            }
            return elementHeight;
        }

        private void OnDrawSubTreeElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var innerRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
            GUI.Box(innerRect, GUIContent.none);
            if (_treeInfo.subTrees.Count > index && _subDrawers.Count > index)
            {
                var info = _treeInfo.subTrees[index];
                var drawer = GetSubDrawer(info);
                var disable = _treeInfo.node is ParentNode parentNode && parentNode.maxChildCount <= index;
                using (var disableScope = new EditorGUI.DisabledGroupScope(disable))
                {
                    var baseInfo = TreeInfoInBase(info);
                    bool green = !EditorApplication.isPlaying && _treeDrawer is BVariantTreeDrawer && CheckTreeInfoChangedSelf(baseInfo, info);
                    using (var colorScope = new ColorScope(green, new Color(0, 1, 0, 0.8f)))
                    {
                        drawer.OnInspectorGUI(innerRect, _deepth + (1 + index));
                    }
                }
            }
        }

        private bool CheckTreeInfoChangedSelf(TreeInfo baseInfo, TreeInfo info)
        {
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
            return false;
        }

        public float GetHeight()
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (_treeInfo.enable)
            {
                if (_treeInfo.condition.enable)
                {
                    height += _conditionDrawer.GetHeight();
                }

                var _needChild = _treeInfo.node is ParentNode || (_treeInfo.subTrees != null && _treeInfo.subTrees.Count > 0);
                if (_needChild)
                {
                    height += _subTreeList.GetHeight();
                }
            }
            return height;
        }

        public void OnInspectorGUI(Rect position, string deepth)
        {
            var fullRect = position;

            this._deepth = deepth;
            var nodeEnableRect = new Rect(position.x, position.y, 20, EditorGUIUtility.singleLineHeight);
            _treeInfo.enable = EditorGUI.Toggle(nodeEnableRect, _treeInfo.enable);

            var labelRect = new Rect(position.x + 20, position.y, 50, EditorGUIUtility.singleLineHeight);
            if (string.IsNullOrEmpty(_deepth))
            {
                EditorGUI.LabelField(labelRect, "Nodes");
            }
            else
            {
                EditorGUI.LabelField(labelRect, _deepth);
                labelRect.width = _deepth.Length * 8;
            }
            var nodeRect = new Rect(position.x + labelRect.width + 25, position.y, position.width - labelRect.width - 60, EditorGUIUtility.singleLineHeight);
            using (var disableScope = new EditorGUI.DisabledGroupScope(!_treeInfo.enable))
            {
                var baseInfo = TreeInfoInBase(_treeInfo);
                var green = !EditorApplication.isPlaying && (_treeDrawer is BVariantTreeDrawer) && CheckTreeInfoChangedSelf(baseInfo, _treeInfo);
                using (var colorScope = new ColorScope(green, Color.green))
                {
                    DrawCreateNodeContent(nodeRect, _treeInfo.node, n =>
                    {
                        RecordUndo("node changed!");
                        _treeInfo.node = n;
                    }, _treeDrawer.target as BTree);
                    DrawNodeState(_treeInfo, nodeRect);
                }


                var yOffset = position.y + nodeRect.height;
                position.x += 30;
                position.width -= 30;

                var _needCondition = true;
                var _needChild = _treeInfo.node is ParentNode || (_treeInfo.subTrees != null && _treeInfo.subTrees.Count > 0);

                using (var disableScop = new EditorGUI.DisabledGroupScope(!_needCondition))
                {
                    var conditionEnableRect = new Rect(position.x + position.width - 30, position.y, 20, EditorGUIUtility.singleLineHeight);
                    _treeInfo.condition.enable = EditorGUI.Toggle(conditionEnableRect, new GUIContent("", "ConditionEnable"), _treeInfo.condition.enable, EditorStyles.radioButton);
                }

                if (_treeInfo.enable)
                {
                    if (_needCondition && _treeInfo.condition.enable)
                    {
                        var conditionRect = new Rect(position.x, yOffset, position.width, _conditionDrawer.GetHeight());
                        _conditionDrawer.OnGUI(conditionRect);

                        var countRect = new Rect(position.x + position.width - 15, yOffset, 20, EditorGUIUtility.singleLineHeight);
                        EditorGUI.LabelField(countRect, $"[{_treeInfo.condition.conditions.Count}]", EditorStyles.miniBoldLabel);

                        yOffset += _conditionDrawer.GetHeight();
                    }

                    if (_needChild)
                    {
                        DrawLineLink(_treeInfo, fullRect, yOffset);
                        var subTreeRect = new Rect(position.x, yOffset, position.width, _subTreeList.GetHeight());
                        _subTreeList.DoList(subTreeRect);
                    }
                    else if (_needCondition && _treeInfo.condition.conditions.Count > 0)
                    {
                        DrawWireBox(_treeInfo, new Rect(fullRect.x, fullRect.y, fullRect.width, fullRect.height));
                    }

                }
            }

            var menuRect = new Rect(position.x - 80, position.y, 60, EditorGUIUtility.singleLineHeight);
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Hierarchy/Add Child"), false, (x) => OnAddSubTreeElement(_subTreeList), 0);
                menu.AddItem(new GUIContent("Hierarchy/Insert Parent"), false, InsertParent, 0);
                menu.AddItem(new GUIContent("Hierarchy/Delete Self"), false, DeleteSelf, 0);
                menu.AddItem(new GUIContent("Hierarchy/Delete All"), false, DeleteAll, 0);
                menu.AddItem(new GUIContent("Reuse/Cut Hierarchy"), false, CopyNode, 1);
                menu.AddItem(new GUIContent("Reuse/Copy Hierarchy"), false, CopyNode, 0);
                if (CopyPasteUtil.copyedTreeInfo != _treeInfo && _treeInfo != null)
                    menu.AddItem(new GUIContent("Reuse/Paste Hierarchy"), false, PasteNode, 0);
                menu.ShowAsContext();
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.D)
            {
                DeleteSelf(0);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.shift && Event.current.keyCode == KeyCode.D)
            {
                DeleteAll(0);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.C)
            {
                CopyNode(0);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.X)
            {
                CopyNode(1);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.V)
            {
                if (CopyPasteUtil.copyedTreeInfo != _treeInfo && _treeInfo != null)
                    PasteNode(0);
            }

            if (labelRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy Node"), false, (x) =>
                {
                    CopyPasteUtil.copyNode = _treeInfo.node;
                }, 0);
                if (CopyPasteUtil.copyNode)
                {
                    menu.AddItem(new GUIContent("Paste Node"), false, (x) =>
                    {
                        RecordUndo("paste node");
                        _treeInfo.node = CopyPasteUtil.copyNode;
                        _changed = true;
                    }, 0);
                }

                menu.ShowAsContext();
            }
        }

        public static void DrawCreateNodeContent<T>(Rect rect, T node, Action<T> onCreate, BTree tree) where T : BaseNode
        {
            var nameRect = new Rect(rect.x, rect.y, rect.width - 50, rect.height);

            bool createTouched = false;
            if (node != null)
            {
                var iconX = nameRect.max.x + 5;
                BTree subTree = null;
                if (node is BTreeNode subNode)
                {
                    subTree = EditorApplication.isPlaying ? subNode.instaneTree : subNode.tree;
                    if (subTree)
                    {
                        var subRect = new Rect(nameRect.xMax - 20, nameRect.y, 20, EditorGUIUtility.singleLineHeight);
                        if(IconCacheUtil.DrawSubTree(subRect, subTree))
                            nameRect.width -= 25;
                    }
                }
                using (var disableScope = new EditorGUI.DisabledScope(tree is BVariantTree))
                {
                    node.name = EditorGUI.TextField(nameRect, node.name, EditorStyles.objectField);
                }
                var iconRect = new Rect(iconX, rect.y, 20, EditorGUIUtility.singleLineHeight);
                IconCacheUtil.DrawIcon(iconRect, node);
            }
            else
            {
                using (var disableScope = new EditorGUI.DisabledScope(tree is BVariantTree))
                {
                    using (var colorScope = new ColorScope(true, Color.red))
                    {
                        if (GUI.Button(nameRect, "Null", EditorStyles.textField))
                        {
                            createTouched = true;
                        }
                    }
                }
            }
            using (var disableScope = new EditorGUI.DisabledScope(tree is BVariantTree))
            {
                var createRect = new Rect(rect.x + rect.width - 20, rect.y, 20, rect.height);
                if (createTouched || GUI.Button(createRect, "", EditorStyles.popup))
                {
                    CreateNodeWindow.Show(Event.current.mousePosition, (node) =>
                    {
                        onCreate?.Invoke(node as T);
                    }, typeof(T), tree);
                }
            }
        }


        private void DrawNodeState(TreeInfo node, Rect rect)
        {
            var color = Color.gray;
            var show = false;
            if (node != null)
            {
                if (node.status == Status.Success)
                {
                    show = true;
                    color = Color.green;
                    if (node.tickCount != _tree.TickCount)
                        color = Color.cyan;
                }
                else if (node.status == Status.Failure)
                {
                    show = true;
                    color = Color.red;
                    if (node.tickCount != _tree.TickCount)
                        color = new Color(1, 0, 1);
                }
                else if (node.status == Status.Running)
                {
                    show = true;
                    color = Color.yellow;
                    if (node.tickCount != _tree.TickCount)
                        color = Color.gray;
                }
                else if (node.status == Status.Interrupt)
                {
                    show = true;
                    color = Color.blue;
                    if (node.tickCount != _tree.TickCount)
                        color = Color.gray;
                }
            }
            if (show)
            {
                using (var colorScope = new ColorGUIScope(true, color))
                {
                    GUI.Box(rect, "");
                }
            }
        }

        private void DrawWireBox(TreeInfo node, Rect rect)
        {
            rect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4);
            var leftRect = new Rect(rect.x, rect.y, 2, rect.height);
            var rightRect = new Rect(rect.x + rect.width - 2, rect.y, 2, rect.height);
            var topRect = new Rect(rect.x, rect.y, rect.width, 2);
            var bottomRect = new Rect(rect.x, rect.y + rect.height - 2, rect.width, 2);
            var color = Color.gray;
            if (node != null)
            {
                if (node.status == Status.Success)
                    color = Color.green;
                else if (node.status == Status.Failure)
                    color = Color.red;
                else if (node.status == Status.Running)
                    color = Color.yellow;
                else if (node.status == Status.Interrupt)
                    color = Color.blue;
            }

            color.a = 0.5f;
            using (var colorScope = new ColorGUIScope(true, color))
            {
                GUI.DrawTexture(leftRect, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(rightRect, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(topRect, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(bottomRect, EditorGUIUtility.whiteTexture);
            }
        }

        private void DrawLineLink(TreeInfo node, Rect rect, float yOffset)
        {
            yOffset = yOffset - rect.y;
            float height = (rect.height - yOffset);
            var percent = (height * 0.5f + yOffset - 1.5f * EditorGUIUtility.singleLineHeight) / rect.height;
            var verticllLine = new Rect(rect.x + 5, rect.y + 15, 3, rect.height * percent);
            var horizontallLine = new Rect(verticllLine.x, verticllLine.yMax, 25, 3);
            var color = Color.gray;
            if (node != null)
            {
                if (node.status == Status.Success)
                    color = Color.green;
                else if (node.status == Status.Failure)
                    color = Color.red;
                else if (node.status == Status.Running)
                    color = Color.yellow;
                else if (node.status == Status.Running)
                    color = Color.blue;
            }
            color.a = 0.5f;
            using (var colorScope = new ColorGUIScope(true, color))
            {
                GUI.DrawTexture(verticllLine, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(horizontallLine, EditorGUIUtility.whiteTexture);
            }
        }

        private void CopyNode(object userData)
        {
            CopyPasteUtil.cut = (userData is int u) && u == 1;
            CopyPasteUtil.copyedTreeInfo = _treeInfo;
            CopyPasteUtil.copyedTreeInfoDrawer = this;
            if (_treeInfo.node)
            {
                CopyPasteUtil.copyNode = _treeInfo.node;
            }
        }

        public void CopyNode(TreeInfo source, TreeInfo target)
        {
            RecordUndo("copy node");
            CopyPasteUtil.CopyTreeInfo(source, target, target);
            CopyPasteUtil.copyedTreeInfo = null;
            RebuildSubTreeList();
            RebuildConditionList();
            _changed = true;

        }

        private void PasteNode(object userData)
        {
            if (CopyPasteUtil.copyedTreeInfo != _treeInfo)
            {
                RecordUndo("paste node");
                CopyNode(CopyPasteUtil.copyedTreeInfo, _treeInfo);
                if (CopyPasteUtil.copyedTreeInfoDrawer != null && CopyPasteUtil.cut)
                {
                    CopyPasteUtil.copyedTreeInfoDrawer.DeleteAll(null);
                }
            }
        }

        private void DeleteAll(object userData)
        {
            if (_parentTree != null)
            {
                RecordUndo("remove all element");
                _parentTree._treeInfo.subTrees.Remove(_treeInfo);
                _parentTree.RebuildSubTreeList();
                _changed = true;
            }
        }

        private void InsertParent(object a)
        {
            RecordUndo("insert parent element");
            var treeInfo = new TreeInfo();
            treeInfo.id = Guid.NewGuid().ToString();
            treeInfo.enable = _treeInfo.enable;
            treeInfo.node = _treeInfo.node;
            treeInfo.condition = _treeInfo.condition;
            treeInfo.subTrees = _treeInfo.subTrees;
            _treeInfo.node = null;
            _treeInfo.condition = new ConditionInfo();
            _treeInfo.subTrees = new List<TreeInfo>() { treeInfo };
            _subDrawers[treeInfo] = new TreeInfoDrawer(_treeDrawer, this, treeInfo);
            RebuildSubTreeList();
            RebuildConditionList();
            _changed = true;
        }

        private void DeleteSelf(object arg)
        {
            RecordUndo("delete self element");
            if (_parentTree != null)
            {
                _parentTree._treeInfo.subTrees.Remove(_treeInfo);
                if (_treeInfo.subTrees != null)
                {
                    _parentTree._treeInfo.subTrees.AddRange(_treeInfo.subTrees);
                }
                _parentTree.RebuildSubTreeList();
                _parentTree.RebuildConditionList();
                _changed = true;
            }
            else
            {
                if (_treeInfo.subTrees.Count > 1)
                {
                    EditorUtility.DisplayDialog("InValid", "root tree can`t delete self,child count>1", "ok");
                }
                else
                {
                    var bTree = _treeDrawer.target as BTree;
                    bTree.rootTree = _treeInfo.subTrees[0];
                    _treeDrawer.RebuildView();
                    _changed = true;
                }
            }
        }

        private void RecordUndo(string flag)
        {
            _treeDrawer.RecordUndo(flag);
        }

        protected TreeInfo TreeInfoInBase(TreeInfo info)
        {
            if (_treeDrawer.target is BVariantTree variantTree)
            {
                return variantTree.baseTree.FindTreeInfo(info.id);
            }
            return null;
        }
    }

    public struct ColorScope : IDisposable
    {
        private Color _oldColor;
        private bool _changeColor;
        public ColorScope(bool active, Color color)
        {
            _changeColor = active;
            _oldColor = GUI.contentColor;

            if (_changeColor)
            {
                GUI.contentColor = color;
            }
        }

        public void Dispose()
        {
            if (_changeColor)
                GUI.contentColor = _oldColor;
        }
    }
    public struct ColorGUIScope : IDisposable
    {
        private Color _oldColor;
        private bool _changeColor;
        public ColorGUIScope(bool active, Color color)
        {
            _changeColor = active;
            _oldColor = GUI.color;
            if (_changeColor)
            {
                GUI.color = color;
            }
        }

        public void Dispose()
        {
            if (_changeColor)
            {
                GUI.color = _oldColor;
            }
        }
    }
    public struct ColorBgScope : IDisposable
    {
        private Color _oldColor;
        private bool _changeColor;
        public ColorBgScope(bool active, Color color)
        {
            _changeColor = active;
            _oldColor = GUI.backgroundColor;
            if (_changeColor)
            {
                GUI.backgroundColor = color;
            }
        }

        public void Dispose()
        {
            if (_changeColor)
            {
                GUI.backgroundColor = _oldColor;
            }
        }
    }
}

