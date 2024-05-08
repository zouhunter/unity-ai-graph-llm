using AIScripting;
using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting
{
    //[CustomNode("VariableInit", group: "AIScripting")]
    public abstract class VariableInitNode : ScriptNodeBase
    {
        public bool executeInEditOnly = false;
        protected override int OutCount => 1;

        protected override IEnumerable<IRef> GetRefVars()
        {
            if(executeInEditOnly)
            {
                if(!Application.isEditor || !Application.isPlaying)
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