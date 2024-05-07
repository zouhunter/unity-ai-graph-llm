using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIScripting
{
    public class AsyncOp
    {
        public float progress { get; protected set; }

        private Action<IScriptGraphNode> _onFinishNode;
        private Action<IScriptGraphNode> _onProgressNode;
        private IScriptGraphNode _node;

        public AsyncOp(IScriptGraphNode target)
        {
            progress = 0;
            _node = target;
        }

        public void RegistComplete(Action<IScriptGraphNode> onFinishNode)
        {
            _onFinishNode = onFinishNode;
            if(progress == 1)
                _onFinishNode?.Invoke(_node);
        }

        public void RegistProgress(Action<IScriptGraphNode> onProgress)
        {
            _onProgressNode = onProgress;
            if(progress != 0)
                _onProgressNode?.Invoke(_node);
        }

        public void SetProgress(int progress)
        {
            this.progress = progress;
        }

        public void SetFinish()
        {
            progress = 1;
            _onFinishNode?.Invoke(_node);
        }
    }
}