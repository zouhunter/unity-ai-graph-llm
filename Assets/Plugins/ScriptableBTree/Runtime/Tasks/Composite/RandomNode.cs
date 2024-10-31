/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 随机执行节点
 *_*/

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Composite
{
    public class RandomNode : CompositeNode
    {
        [SerializeField]
        private MatchType _abortType;
        public MatchType abortType => _abortType;


        public int limitCount = 1;

        private int _matchCount;
        private int[] _randomIndexs;
        private System.Random _rand;

        public void Shuffle(int[] array)
        {
            if(_rand == null)
                _rand = new System.Random();

            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = _rand.Next(); // 随机选择一个元素
                int temp = array[k]; // 进行交换
                array[k] = array[n];
                array[n] = temp;
            }
        }

        protected override Status OnUpdate(TreeInfo info)
        {
            var childCount = GetChildCount(info);
            if (childCount == 0 || limitCount == 0)
                return Status.Inactive;

            _matchCount = 0;
            var calcuteCount = limitCount < 0 ? childCount : Mathf.Min(limitCount, childCount);
            if (_randomIndexs == null || _randomIndexs.Length != calcuteCount)
            {
                _randomIndexs = new int[calcuteCount];
                for (int i = 0; i < _randomIndexs.Length; i++)
                    _randomIndexs[i] = i;
            }
            Shuffle(_randomIndexs);
            for (int i = 0; i < calcuteCount; i++)
            {
                var child = GetChild(info,i);
                var childStatus = child.node?.Execute(child) ?? Status.Inactive;

                switch (childStatus)
                {
                    case Status.Inactive:
                        continue;
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        if (abortType == MatchType.AnyFailure)
                            return Status.Success;
                        else if (abortType == MatchType.AllSuccess)
                            return Status.Failure;
                        _matchCount++;
                        break;
                    case Status.Success:
                        if (abortType == MatchType.AnySuccess)
                            return Status.Success;
                        else if (abortType == MatchType.AllFailure)
                            return Status.Failure;
                        _matchCount++;
                        break;
                    default:
                        break;
                }
            }

            if (_matchCount == calcuteCount && (abortType == MatchType.AllSuccess || abortType == MatchType.AllFailure))
                return Status.Success;

            return Status.Failure;
        }
    }
}
