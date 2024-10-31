using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    public abstract class ParentNode : BaseNode
    {
        public virtual int maxChildCount { get => int.MaxValue; }

        /// <summary>
        /// 获取有效子树
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int GetChildCount(TreeInfo info)
        {
            if (info.subTrees == null)
                return 0;
            int count = 0;
            for (int i = 0; i < info.subTrees.Count; i++) {
                var subTree = info.subTrees[i];
                if(subTree != null && subTree.enable)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 查找子节点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual TreeInfo GetChild(TreeInfo info, int index)
        {
            if (info.subTrees == null)
                return null;
            var id = -1;
            foreach (TreeInfo child in info.subTrees)
            {
                if (child.enable)
                    id++;
                if (id == index)
                    return child;
            }
            return null;
        }
    }
}
