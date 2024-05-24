using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;

using UnityEngine;

namespace AIScripting.Batch
{
    [CustomNode("StrListBatch", group: Define.GROUP)]
    public class StrListBatchNode : ScriptNodeBase
    {
        public Ref<List<string>> list;
        public Ref<string> export;
        private int _index;

        protected override void OnProcess()
        {

        }
    }
}