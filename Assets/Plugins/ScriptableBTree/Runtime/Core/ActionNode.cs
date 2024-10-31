using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    public abstract class ActionNode : BaseNode
    {
        public override Status Execute(TreeInfo info)
        {
            var result = base.Execute(info);
            if (_conditionFaliure)
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
