/*-*-* Copyright (c) mateai@wekoi
 * Author: 
 * Creation Date: 2024-05-31
 * Version: 1.0.0
 * Description: 
 *_*/

using System;
using System.Collections;
using UFrame.NodeGraph;

namespace AIScripting.Waiting
{
    [CustomNode("WaitSecond", group: Define.GROUP)]
    public class WaitSecondNode : WaitBaseNode
    {
        public float second = 1;

        protected override IEnumerator Wait(Action onFinish)
        {
            yield return new WaitForSeconds(second);
            onFinish?.Invoke();
        }
    }
}
