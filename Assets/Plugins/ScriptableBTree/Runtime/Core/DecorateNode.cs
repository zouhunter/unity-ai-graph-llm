/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 装饰器
 *_*/
namespace MateAI.ScriptableBehaviourTree
{
    /// <summary>
    /// 装饰器
    /// </summary>
    public abstract class DecorateNode : ParentNode
    {
        public override int maxChildCount => 1;

        protected virtual Status ExecuteChild(TreeInfo info)
        {
            var childNode = GetChild(info,0);
            if (childNode != null)
            {
               return childNode.node.Execute(childNode);
            }
            return Status.Inactive;
        }
    }
}
