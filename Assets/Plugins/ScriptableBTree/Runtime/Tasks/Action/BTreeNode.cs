/*-*-* Copyright (c) webxr@zht
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: Returns a Status of appoint.
 *_*/

using MateAI.ScriptableBehaviourTree;
using MateAI;
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree.Actions
{
    [NodePath("BTreeNode")]
    public class BTreeNode : ActionNode
    {
        public BTree tree;
        [SerializeField]
        private BTree _instanceTree;
        public BTree instaneTree => _instanceTree;

        public override void SetOwner(BTree owner)
        {
            base.SetOwner(owner);
            if (tree)
                _instanceTree = UnityEngine.Object.Instantiate(tree);
            if (_instanceTree)
                _instanceTree.SetOwnerDeepth(_instanceTree.rootTree, owner);
        }

        protected override void OnReset()
        {
            base.OnReset();
            _instanceTree?.OnReset();
        }

        protected override Status OnUpdate()
        {
            return _instanceTree?.Tick() ?? Status.Failure;
        }

        protected override void OnClear()
        {
            base.OnClear();
            if(_instanceTree != null)
                _instanceTree.CleanDeepth(_instanceTree.rootTree);
        }
    }
}
