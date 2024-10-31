/*-*-* Copyright (c) mateai@wekoi
 * Author: zouhunter
 * Creation Date: 2024-07-24
 * Version: 1.0.0
 * Description: 
 *_*/

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace MateAI.ScriptableBehaviourTree
{
    public class ConditionDrawer
    {

        private ReorderableList _conditionList;
        private TreeInfo _treeInfo;
        private BTree _tree;
        private Dictionary<int, ReorderableList> _subConditionListMap = new Dictionary<int, ReorderableList>();
        public ConditionDrawer(BTree bTree, TreeInfo info)
        {
            _tree = bTree;
            _treeInfo = info;
            bool canAddOrDelete = !(bTree is BVariantTree);
            if (_treeInfo.condition.conditions == null)
                _treeInfo.condition.conditions = new List<ConditionItem>();
            _conditionList = new ReorderableList(_treeInfo.condition.conditions, typeof(ConditionNode), true, true, canAddOrDelete, canAddOrDelete);
            _conditionList.drawHeaderCallback = OnDrawConditionHead;
            _conditionList.onAddCallback = OnAddCondition;
            _conditionList.onRemoveCallback = OnDeleteCondition;
            _conditionList.elementHeightCallback = OnDrawConditonHight;
            _conditionList.drawElementCallback = OnDrawCondition;
        }

        public float GetHeight()
        {
            return _conditionList.GetHeight();
        }

        public void OnGUI(Rect rect)
        {
            _conditionList.DoList(rect);
        }

        protected TreeInfo TreeInfoInBase(TreeInfo info)
        {
            if (_tree is BVariantTree variantTree && variantTree.baseTree)
            {
                return variantTree.baseTree.FindTreeInfo(info.id);
            }
            return null;
        }

        private void OnDrawConditionHead(Rect rect)
        {
            var color = Color.yellow;
            if (_treeInfo.node && _treeInfo.status == Status.Success)
                color = Color.green;

            using (var c = new ColorScope(true, Color.yellow))
                EditorGUI.LabelField(rect, "Conditions");

            var matchRect = new Rect(rect.x + rect.width - 75, rect.y, 75, EditorGUIUtility.singleLineHeight);
            _treeInfo.condition.matchType = (MatchType)EditorGUI.EnumPopup(matchRect, _treeInfo.condition.matchType, EditorStyles.linkLabel);
        }

        private bool CheckConditionChanged(int index,out bool selfChanged,out bool matchTypeChanged)
        {
            var baseTreeInfo = TreeInfoInBase(_treeInfo);
            bool changed = false;
            matchTypeChanged = false;
            selfChanged = false;
            if (_tree is BVariantTree && !EditorApplication.isPlaying && baseTreeInfo?.condition?.conditions != null && _treeInfo?.condition?.conditions != null)
            {
                if (_treeInfo.condition.conditions.Count > index && baseTreeInfo.condition.conditions.Count > index)
                {
                    if ((baseTreeInfo.condition.conditions[index].subEnable != _treeInfo.condition.conditions[index].subEnable))
                    {
                        selfChanged = true;
                        changed = true;
                    }
                    if (baseTreeInfo.condition.conditions[index].matchType != _treeInfo.condition.conditions[index].matchType)
                    {
                        matchTypeChanged = true;
                        changed = true;
                    }
                    if (baseTreeInfo.condition.conditions[index].state != _treeInfo.condition.conditions[index].state)
                    {
                        selfChanged = true;
                        changed = true;
                    }
                }
                else if (baseTreeInfo.condition.conditions.Count <= index && index != 0)
                {
                    changed = true;
                }
            }
            return changed;
        }


        private bool CheckSubConditionChanged(int index, int subIndex)
        {
            var baseTreeInfo = TreeInfoInBase(_treeInfo);
            bool changed = false;
            if (_tree is BVariantTree && !EditorApplication.isPlaying && baseTreeInfo?.condition?.conditions != null && _treeInfo?.condition?.conditions != null)
            {
                var baseCondtions = baseTreeInfo?.condition?.conditions[index];
                var condtions = _treeInfo?.condition?.conditions[index];
                if (baseCondtions.subConditions.Count > subIndex && condtions.subConditions.Count > subIndex)
                {
                    changed = baseCondtions.subConditions[subIndex].state != condtions.subConditions[subIndex].state;
                }
            }
            return changed;
        }


        private float OnDrawConditonHight(int index)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (_treeInfo.condition.conditions.Count > index)
            {
                var condition = _treeInfo.condition.conditions[index];
                if (condition == null)
                    _treeInfo.condition.conditions[index] = condition = new ConditionItem();

                if (condition.subEnable && condition.state < 2)
                {
                    height += EditorGUIUtility.singleLineHeight * 2.5f;
                    if (condition.subConditions != null && condition.subConditions.Count > 0)
                    {
                        height += EditorGUIUtility.singleLineHeight * condition.subConditions.Count;
                    }
                }
            }
            return height;
        }

        private void OnDrawCondition(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_treeInfo.condition.conditions == null || _treeInfo.condition.conditions.Count <= index)
                return;
            var condition = _treeInfo.condition.conditions[index];
            var inverseRect = new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight);
            var normaIcon = EditorGUIUtility.IconContent("d_toggle_on_focus").image;
            var inverseIcon = IconCacheUtil.GetTextureByGUID("dd3247368b7f1bc43baf81039a01f4ee");
            var inValidIcon = EditorGUIUtility.IconContent("d_console.erroricon").image;
            var conditionIcon = (condition.state == 0) ? normaIcon : (condition.state == 1) ? inverseIcon : inValidIcon;
            if (GUI.Button(inverseRect, conditionIcon))
                condition.state = (++condition.state) % 3;

            var conditionVariantChanged = CheckConditionChanged(index,out var conditonSelfChanged,out var condtionMatchTypeChanged);

            if (condition == null)
                condition = _treeInfo.condition.conditions[index] = new ConditionItem();
            var objectRect = new Rect(rect.x + 25, rect.y, rect.width - 55, EditorGUIUtility.singleLineHeight);
            var enableRect = new Rect(rect.x + rect.width - 20, rect.y, 20, EditorGUIUtility.singleLineHeight);

            using (var disableScope = new EditorGUI.DisabledScope(condition.state > 1))
            {
                using (var colorScope = new ColorScope(conditonSelfChanged, Color.green))
                {
                    TreeInfoDrawer.DrawCreateNodeContent(objectRect, condition.node, n =>
                    {
                        RecordUndo("condition node changed!");
                        condition.node = n;
                    }, _tree);
                    DrawNodeState(condition.tickCount,condition.status, objectRect);
                    condition.subEnable = EditorGUI.Toggle(enableRect, condition.subEnable, EditorStyles.radioButton);
                }
            }

            var subConditionEnable = condition.subEnable && condition.state < 2;
            if (subConditionEnable)
            {
                var hashCode = condition.GetHashCode();
                if (!_subConditionListMap.TryGetValue(hashCode, out var subConditionList))
                {
                    if (condition.subConditions == null)
                        condition.subConditions = new List<SubConditionItem>();
                    subConditionList = _subConditionListMap[hashCode] = new ReorderableList(condition.subConditions, typeof(ConditionNode), true, true, true, true);
                    subConditionList.headerHeight = 0;
                    subConditionList.drawElementCallback = (subRect, subIndex, subIsActive, subIsFocused) =>
                    {
                        var subNode = condition.subConditions[subIndex];
                        if (subNode == null)
                            subNode = new SubConditionItem();
                        var subCondtionChanged = CheckSubConditionChanged(index, subIndex);

                        var subInverseRect = new Rect(subRect.x, subRect.y, 20, EditorGUIUtility.singleLineHeight);
                        var subConditionIcon = (subNode.state == 0) ? normaIcon : (subNode.state == 1) ? inverseIcon : inValidIcon;
                        using(var colorGroup2 = new ColorScope(subCondtionChanged, Color.green))
                        {
                            if (GUI.Button(subInverseRect, subConditionIcon))
                                subNode.state = (++subNode.state) % 3;

                            using (var disableScope2 = new EditorGUI.DisabledGroupScope(subNode.state > 1))
                            {
                                var subObjectRect = new Rect(subRect.x + 25, subRect.y, subRect.width - 25, EditorGUIUtility.singleLineHeight);
                                TreeInfoDrawer.DrawCreateNodeContent(subObjectRect, subNode.node, n =>
                                {
                                    RecordUndo("condition sub node changed!");
                                    subNode.node = n;
                                }, _tree);
                                DrawNodeState(subNode.tickCount,subNode.status, subObjectRect);
                            }

                        }

                        condition.subConditions[subIndex] = subNode;
                        var subMenuRect = new Rect(subRect.x - 100, subRect.y, 100, EditorGUIUtility.singleLineHeight);
                        if (subMenuRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
                        {
                            var menu = new GenericMenu();
                            if (CopyPasteUtil.copyNode && CopyPasteUtil.copyNode is ConditionNode cdn)
                            {
                                menu.AddItem(new GUIContent("Paste"), false, (x) =>
                                {
                                    RecordUndo("paste node");
                                    condition.subConditions[subIndex].node = cdn;
                                }, 0);
                            }
                            if (subNode.node)
                            {
                                menu.AddItem(new GUIContent("Copy"), false, (x) =>
                                {
                                    CopyPasteUtil.copyNode = subNode.node;
                                }, 0);
                            }
                            menu.ShowAsContext();
                        }
                    };
                    subConditionList.onAddCallback = (subList) =>
                    {
                        var baseTreeInfo = TreeInfoInBase(_treeInfo);
                        if (baseTreeInfo != null)
                        {
                            EditorUtility.DisplayDialog("Error", "Can't add base tree sub conditions", "OK");
                            return;
                        }
                        condition.subConditions.Add(null);
                    };
                    subConditionList.onRemoveCallback = (subList) =>
                    {
                        var baseTreeInfo = TreeInfoInBase(_treeInfo);
                        if (baseTreeInfo != null)
                        {
                            EditorUtility.DisplayDialog("Error", "Can't delete base tree sub conditions", "OK");
                            return;
                        }
                        RecordUndo("remove sub condition element");
                        condition.subConditions.RemoveAt(subList.index);
                    };
                }
                subConditionList.DoList(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, subConditionList.GetHeight()));
            }

            if (subConditionEnable)
            {
                var verticlOffset = EditorGUIUtility.singleLineHeight;
                if (condition.subConditions != null && condition.subConditions.Count > 0)
                {
                    verticlOffset += (EditorGUIUtility.singleLineHeight + 4) * condition.subConditions.Count;
                }

                if (condition.subConditions != null && condition.subConditions.Count > 0)
                {
                    var matchTypeRect = new Rect(rect.x + 20, rect.y + verticlOffset, 75, EditorGUIUtility.singleLineHeight);
                    using (var colorScope = new ColorScope(condtionMatchTypeChanged, new Color(0, 1, 0, 0.8f)))
                    {
                        condition.matchType = (MatchType)EditorGUI.EnumPopup(matchTypeRect, condition.matchType, EditorStyles.linkLabel);
                    }
                }
            }

            var subEnableRect1 = new Rect(rect.x - 100, rect.y, 100, EditorGUIUtility.singleLineHeight);
            if (subEnableRect1.Contains(Event.current.mousePosition))
            {
                if(Event.current.type == EventType.MouseUp && Event.current.button == 1)
                {
                    var menu = new GenericMenu();
                    if (CopyPasteUtil.copyNode && CopyPasteUtil.copyNode is ConditionNode cdn)
                    {
                        menu.AddItem(new GUIContent("Paste"), false, (x) =>
                        {
                            RecordUndo("paste node");
                            condition.node = cdn;
                        }, 0);
                    }
                    if (condition.node)
                    {
                        menu.AddItem(new GUIContent("Copy"), false, (x) =>
                        {
                            CopyPasteUtil.copyNode = condition.node;
                        }, 0);
                    }
                    menu.ShowAsContext();
                }
                else if(Event.current.control && Event.current.keyCode == KeyCode.C)
                {
                    CopyPasteUtil.copyNode = condition.node;
                }
                else if (Event.current.control && Event.current.keyCode == KeyCode.V && CopyPasteUtil.copyNode is ConditionNode cdn)
                {
                    RecordUndo("paste node");
                    condition.node = cdn;
                }
            }
        }

        private void DrawNodeState(int tickCount,Status status, Rect rect)
        {
            var color = Color.gray;
            var show = false;
            if (status == Status.Success)
            {
                show = true;
                color = Color.green;
                if (tickCount != _tree.TickCount)
                    color = Color.cyan;
            }
            else if (status == Status.Failure)
            {
                show = true;
                color = Color.red;
                if (tickCount != _tree.TickCount)
                    color = new Color(1, 0, 1);
            }
            else if (status == Status.Running)
            {
                show = true;
                color = Color.yellow;
                if (tickCount != _tree.TickCount)
                    color = Color.gray;
            }
            if (show)
            {
                using (var colorScope = new ColorGUIScope(true, color))
                {
                    GUI.Box(rect, "");
                }
            }
        }

        private void OnDeleteCondition(ReorderableList list)
        {
            var baseTreeInfo = TreeInfoInBase(_treeInfo);
            if (baseTreeInfo != null)
            {
                EditorUtility.DisplayDialog("Error", "Can't delete base tree sub condition", "OK");
                return;
            }

            RecordUndo("remove condition element");
            _treeInfo.condition.conditions.RemoveAt(list.index);
        }

        private void OnAddCondition(ReorderableList list)
        {
            var baseTreeInfo = TreeInfoInBase(_treeInfo);
            if (baseTreeInfo != null)
            {
                EditorUtility.DisplayDialog("Error", "Can't add base tree sub condition", "OK");
                return;
            }
            RecordUndo("add condition element");
            _treeInfo.condition.conditions.Add(new ConditionItem());
        }

        private void RecordUndo(string info)
        {
            Undo.RecordObject(_tree, info);
        }
    }
}

