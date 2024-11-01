/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 复合节点
 *_*/

using System;

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    public abstract class CompositeNode : ParentNode
    {
        [PrimaryArg(Status.Success,Status.Failure)]
        public Status matchStatus;
    }
}
