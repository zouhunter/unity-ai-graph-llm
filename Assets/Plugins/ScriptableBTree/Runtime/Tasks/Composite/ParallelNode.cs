/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: ����ִ�У��κ�ƥ�䣬���سɹ�
 *_*/
namespace MateAI.ScriptableBehaviourTree.Composite
{
    /// <summary>
    /// ���нڵ�
    /// </summary>
    public class ParallelNode : CompositeNode
    {
        protected override Status OnUpdate(TreeInfo info)
        {
            var status = Status.Inactive;
            if (info.subTrees == null || info.subTrees.Count == 0)
                return status;
            var complete = true;
            for (int i = 0; i < info.subTrees.Count; i++)
            {
                var child = info.subTrees[i];
                if (!child.enable || child.node == null)
                    continue;

                var childStatus = child.node.Execute(child);
                if (childStatus == Status.Running)
                    complete = false;
                else if (childStatus == matchStatus)
                    status = Status.Success;
                else if(childStatus == Status.Success || childStatus == Status.Failure)
                    status = Status.Failure;
            }
            return complete ? status : Status.Running;
        }
    }
}
