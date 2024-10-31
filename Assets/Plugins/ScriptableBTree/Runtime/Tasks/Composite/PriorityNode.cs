/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 优先执行节点
 *_*/

using System;

using UnityEngine;


namespace MateAI.ScriptableBehaviourTree.Composite
{
    public class PriorityNode : SequenceNode
    {
        private int[] _priorityIndexs;

        protected override Status OnUpdate(TreeInfo info)
        {
            RefreshPriority(info);
            return base.OnUpdate();
        }

        public void RefreshPriority(TreeInfo info)
        {
            var childCount = GetChildCount(info);
            if (_priorityIndexs == null)
            {
                _priorityIndexs = new int[childCount];
                for (int i = 0; i < _priorityIndexs.Length; i++)
                {
                    _priorityIndexs[i] = i;
                }
            }

            for (int i = 0; i < childCount; i++)
            {
                var priority = GetChild(info,i).node.Priority;
                for (int j = 0; j < _priorityIndexs.Length; j++)
                {
                    if (i == _priorityIndexs[j])
                        break;

                    var lastPriority = GetChild(info, _priorityIndexs[j]).node.Priority;
                    if (priority > lastPriority)
                    {
                        var indexI = Array.IndexOf(_priorityIndexs, i);
                        if (indexI > j)
                        {
                            _priorityIndexs[indexI] = _priorityIndexs[j];
                            _priorityIndexs[j] = i;
                        }
                    }
                }
            }
        }
    }
}
