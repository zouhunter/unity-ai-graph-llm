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
    [CustomNode("Waiting/WaitFrame")]
    public class WaitFrameNode : WaitBaseNode
    {
        public int frameCount = 1;
        protected override IEnumerator Wait(Action onFinish)
        {
            int counter = frameCount;
            while (counter--> 0)
            {
                yield return null;
            }
            onFinish?.Invoke();
        }
    }
}

