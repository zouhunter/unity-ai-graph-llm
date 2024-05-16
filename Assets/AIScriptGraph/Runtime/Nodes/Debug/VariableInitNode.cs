using AIScripting;
using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Debugger
{
    //[CustomNode("VariableInit", group: Define.GROUP)]
    public abstract class VariableInitNode : ScriptNodeBase
    {
        public bool executeInEditOnly = false;
        protected override IEnumerable<IRef> GetRefVars()
        {
            if(executeInEditOnly)
            {
                if(!Application.isEditor || Application.isPlaying)
                {
                    return null;
                }
            }
            return base.GetRefVars();
        }
  
        protected override void OnProcess()
        {
            DoFinish();
        }
    }
}