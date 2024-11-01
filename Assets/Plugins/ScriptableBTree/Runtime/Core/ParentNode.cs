using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    public abstract class ParentNode : BaseNode
    {
        public virtual int maxChildCount { get => int.MaxValue; }
    }
}
