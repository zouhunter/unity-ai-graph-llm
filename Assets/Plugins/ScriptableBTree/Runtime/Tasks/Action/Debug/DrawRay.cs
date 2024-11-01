/*-*-* Copyright (c) webxr@zht
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: ������
 *_*/
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Actions
{
    [Tooltip("Debug/������")]
    public class DrawRay : ActionNode
    {
        public Ref<Vector3> start;
        public Ref<Vector3> direction;
        public Ref<Color> color;

        protected override Status OnUpdate(TreeInfo info)
        {
            Debug.DrawRay(start.Value, direction.Value, color.Value);
            return Status.Success;
        }
    }
}

