using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;

using UnityEngine;

namespace AIScripting.Batch
{
    [CustomNode("StrDictBatch", group: Define.GROUP)]
    public class StrDictBatchNode : ScriptNodeBase
    {
        public Ref<Dictionary<string,string>> strDict;
        public Ref<string> exportKey;
        public Ref<string> exportValue;
        private int _index;
        private List<string> _usedKeys;

        public override void ResetGraph(AIScriptGraph graph)
        {
            base.ResetGraph(graph);
            _usedKeys = new List<string>();
        }

        protected override void OnProcess()
        {
            string currentKey = null;
            foreach (var item in strDict.Value)
            {
                if(_usedKeys.Contains(item.Key))
                    continue;
                _usedKeys.Add(item.Key);

                currentKey = item.Key;
                break;
            }
            if(!string.IsNullOrEmpty(currentKey))
            {
                exportKey.SetValue(currentKey);
                exportValue.SetValue(strDict.Value[currentKey]);
                DoFinish(true);
            }
            else
            {
                exportKey.SetValue(null);
                exportValue.SetValue(null);
                DoFinish(false);
            }
        }
    }
}