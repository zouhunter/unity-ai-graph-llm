using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    public abstract class ActionNode : BaseNode
    {
        protected override Status OnUpdate(TreeInfo info)
        {
            var result = base.OnUpdate(info);

            if (result == Status.Failure)
                return result;

            if (info.subTrees == null || info.subTrees.Count == 0)
                return result;

            foreach (var subTree in info.subTrees)
            {
                if (subTree.enable)
                {
                    try
                    {
                        subTree.node.Execute(subTree);
                    }
                    catch (System.Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }
            return result;
        }
    }
}
