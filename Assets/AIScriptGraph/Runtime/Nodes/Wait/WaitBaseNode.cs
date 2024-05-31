/*-*-* Copyright (c) Work@uframe
 * Author: 
 * Creation Date: 2024-05-31
 * Version: 1.0.0
 * Description: 
 *_*/

using UFrame.NodeGraph;
using System.Collections;
using System;

namespace AIScripting
{

    public abstract class WaitBaseNode : ScriptNodeBase
    {
        protected override void OnProcess()
        {
            Owner.StartCoroutine(Wait(() =>
            {
                DoFinish(true);
            }));
        }
        protected abstract IEnumerator Wait(Action onFinish);
    }
}

