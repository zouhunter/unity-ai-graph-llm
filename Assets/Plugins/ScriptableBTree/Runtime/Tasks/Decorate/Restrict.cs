/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 执行同时限制次数
 *_*/

using System.Collections.Generic;

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Decorates
{
    [AddComponentMenu("BehaviourTree/Decorate/Restrict")]
    public class Restrict : DecorateNode
    {
        //最大访问次数
        public Ref<int> maxAccessCount;
        //正在执行的任务
        public Ref<List<TreeInfo>> executingTask;

        /// <summary>
        /// 获取访问权限
        /// </summary>
        /// <returns></returns>
        protected bool GetAccess()
        {
            //if(executingTask.Value.Contains(TreeInfo))
            //{
            //    return true;
            //}
            //if (executingTask.Value.Count < maxAccessCount.Value)
            //{
            //    executingTask.Value.Add(TreeInfo);
            //    return true;
            //}
            return false;
        }

        /// <summary>
        /// 执行子节点
        /// </summary>
        /// <returns></returns>
        protected override Status OnUpdate()
        {
            if(GetAccess())
            {
                //var childResult = base.ExecuteChild();
                //if(childResult == Status.Success || childResult == Status.Failure)
                //{
                //    executingTask.Value.Remove(TreeInfo);
                //}
                //return childResult;
            }
            return Status.Running;
        }
    }
}
